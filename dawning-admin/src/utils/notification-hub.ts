import * as signalR from '@microsoft/signalr';
import { getToken } from '@/utils/auth';

// 通知类型
export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  createdAt: string;
  data?: Record<string, unknown>;
}

// 告警通知
export interface AlertNotification extends Notification {
  severity: 'info' | 'warning' | 'error' | 'critical';
  ruleId?: string;
  ruleName?: string;
  source?: string;
  value?: number;
  threshold?: number;
}

// 系统消息
export interface SystemMessage extends Notification {
  requireAcknowledge: boolean;
  priority: 'low' | 'normal' | 'high' | 'urgent';
  expiresAt?: string;
}

// 连接状态
export type ConnectionState = 'connecting' | 'connected' | 'disconnected' | 'reconnecting';

// 事件回调类型
type NotificationCallback = (notification: Notification) => void;
type AlertCallback = (alert: AlertNotification) => void;
type SystemMessageCallback = (message: SystemMessage) => void;
type ConnectionStateCallback = (state: ConnectionState) => void;

class NotificationHubService {
  private connection: signalR.HubConnection | null = null;
  private connectionState: ConnectionState = 'disconnected';
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;

  // 事件监听器
  private notificationListeners: Set<NotificationCallback> = new Set();
  private alertListeners: Set<AlertCallback> = new Set();
  private systemMessageListeners: Set<SystemMessageCallback> = new Set();
  private connectionStateListeners: Set<ConnectionStateCallback> = new Set();

  /**
   * 初始化并连接到 SignalR Hub
   */
  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log('[SignalR] Already connected');
      return;
    }

    const token = getToken();
    if (!token) {
      console.warn('[SignalR] No auth token, skipping connection');
      return;
    }

    const hubUrl = `${import.meta.env.VITE_API_BASE_URL || ''}/hubs/notification`;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => getToken() || '',
        transport: signalR.HttpTransportType.WebSockets,
        skipNegotiation: true,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          this.reconnectAttempts = retryContext.previousRetryCount;
          if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
            // 指数退避：1s, 2s, 4s, 8s, 16s, 最大 30s
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          }
          return null; // 停止重连
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // 注册事件处理器
    this.registerEventHandlers();

    // 连接状态监听
    this.connection.onreconnecting(() => {
      this.setConnectionState('reconnecting');
    });

    this.connection.onreconnected(() => {
      this.reconnectAttempts = 0;
      this.setConnectionState('connected');
    });

    this.connection.onclose(() => {
      this.setConnectionState('disconnected');
    });

    try {
      this.setConnectionState('connecting');
      await this.connection.start();
      this.setConnectionState('connected');
      console.log('[SignalR] Connected successfully');
    } catch (error) {
      console.error('[SignalR] Connection failed:', error);
      this.setConnectionState('disconnected');
      throw error;
    }
  }

  /**
   * 断开连接
   */
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.setConnectionState('disconnected');
      console.log('[SignalR] Disconnected');
    }
  }

  /**
   * 订阅频道
   */
  async subscribe(channel: 'alerts' | 'system' | 'monitoring' | 'audit'): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('Subscribe', channel);
      console.log(`[SignalR] Subscribed to channel: ${channel}`);
    }
  }

  /**
   * 取消订阅频道
   */
  async unsubscribe(channel: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('Unsubscribe', channel);
      console.log(`[SignalR] Unsubscribed from channel: ${channel}`);
    }
  }

  /**
   * 确认通知
   */
  async acknowledgeNotification(notificationId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('AcknowledgeNotification', notificationId);
    }
  }

  /**
   * 注册事件处理器
   */
  private registerEventHandlers(): void {
    if (!this.connection) return;

    // 接收普通通知
    this.connection.on('NotificationReceived', (notification: Notification) => {
      this.notificationListeners.forEach((callback) => callback(notification));
    });

    // 接收告警
    this.connection.on('AlertReceived', (alert: AlertNotification) => {
      this.alertListeners.forEach((callback) => callback(alert));
      // 同时触发普通通知
      this.notificationListeners.forEach((callback) => callback(alert));
    });

    // 接收系统消息
    this.connection.on('SystemMessage', (message: SystemMessage) => {
      this.systemMessageListeners.forEach((callback) => callback(message));
      // 同时触发普通通知
      this.notificationListeners.forEach((callback) => callback(message));
    });

    // 通知确认回调
    this.connection.on('NotificationAcknowledged', (notificationId: string) => {
      console.log(`[SignalR] Notification acknowledged: ${notificationId}`);
    });
  }

  /**
   * 设置连接状态
   */
  private setConnectionState(state: ConnectionState): void {
    this.connectionState = state;
    this.connectionStateListeners.forEach((callback) => callback(state));
  }

  /**
   * 获取当前连接状态
   */
  getConnectionState(): ConnectionState {
    return this.connectionState;
  }

  /**
   * 添加通知监听器
   */
  onNotification(callback: NotificationCallback): () => void {
    this.notificationListeners.add(callback);
    return () => this.notificationListeners.delete(callback);
  }

  /**
   * 添加告警监听器
   */
  onAlert(callback: AlertCallback): () => void {
    this.alertListeners.add(callback);
    return () => this.alertListeners.delete(callback);
  }

  /**
   * 添加系统消息监听器
   */
  onSystemMessage(callback: SystemMessageCallback): () => void {
    this.systemMessageListeners.add(callback);
    return () => this.systemMessageListeners.delete(callback);
  }

  /**
   * 添加连接状态监听器
   */
  onConnectionStateChange(callback: ConnectionStateCallback): () => void {
    this.connectionStateListeners.add(callback);
    return () => this.connectionStateListeners.delete(callback);
  }
}

// 单例导出
export const notificationHub = new NotificationHubService();
export default notificationHub;
