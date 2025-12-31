import * as signalR from '@microsoft/signalr';
import { getToken } from '@/utils/auth';

// Notification type
export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  createdAt: string;
  data?: Record<string, unknown>;
}

// Alert notification
export interface AlertNotification extends Notification {
  severity: 'info' | 'warning' | 'error' | 'critical';
  ruleId?: string;
  ruleName?: string;
  source?: string;
  value?: number;
  threshold?: number;
}

// System message
export interface SystemMessage extends Notification {
  requireAcknowledge: boolean;
  priority: 'low' | 'normal' | 'high' | 'urgent';
  expiresAt?: string;
}

// Log entry
export interface LogEntry {
  id: string;
  timestamp: string;
  level: 'Information' | 'Warning' | 'Error' | 'Debug' | 'Trace' | 'Critical';
  message: string;
  exception?: string;
  requestPath?: string;
  requestMethod?: string;
  statusCode?: number;
  userId?: string;
  username?: string;
  ipAddress?: string;
}

// Connection state
export type ConnectionState = 'connecting' | 'connected' | 'disconnected' | 'reconnecting';

// Log channel type
export type LogChannel = 'logs_all' | 'logs_error' | 'logs_warning' | 'logs_info';

// Notification channel type
export type NotificationChannel = 'alerts' | 'system' | 'monitoring' | 'audit';

// Event callback types
type NotificationCallback = (notification: Notification) => void;
type AlertCallback = (alert: AlertNotification) => void;
type SystemMessageCallback = (message: SystemMessage) => void;
type LogEntryCallback = (log: LogEntry) => void;
type ConnectionStateCallback = (state: ConnectionState) => void;

class NotificationHubService {
  private connection: signalR.HubConnection | null = null;
  private connectionState: ConnectionState = 'disconnected';
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;

  // Event listeners
  private notificationListeners: Set<NotificationCallback> = new Set();
  private alertListeners: Set<AlertCallback> = new Set();
  private systemMessageListeners: Set<SystemMessageCallback> = new Set();
  private logEntryListeners: Set<LogEntryCallback> = new Set();
  private connectionStateListeners: Set<ConnectionStateCallback> = new Set();

  /**
   * Initialize and connect to SignalR Hub
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
            // Exponential backoff: 1s, 2s, 4s, 8s, 16s, max 30s
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          }
          return null; // Stop reconnecting
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Register event handlers
    this.registerEventHandlers();

    // Connection state listening
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
   * Disconnect
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
   * Subscribe to channel
   */
  async subscribe(channel: NotificationChannel | LogChannel): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('Subscribe', channel);
      console.log(`[SignalR] Subscribed to channel: ${channel}`);
    }
  }

  /**
   * Subscribe to log channel (admin only)
   */
  async subscribeToLogs(level: 'all' | 'error' | 'warning' | 'info' = 'all'): Promise<void> {
    const channel = `logs_${level}` as LogChannel;
    await this.subscribe(channel);
  }

  /**
   * Unsubscribe from channel
   */
  async unsubscribe(channel: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('Unsubscribe', channel);
      console.log(`[SignalR] Unsubscribed from channel: ${channel}`);
    }
  }

  /**
   * Unsubscribe from log channel
   */
  async unsubscribeFromLogs(level: 'all' | 'error' | 'warning' | 'info' = 'all'): Promise<void> {
    const channel = `logs_${level}`;
    await this.unsubscribe(channel);
  }

  /**
   * Acknowledge notification
   */
  async acknowledgeNotification(notificationId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('AcknowledgeNotification', notificationId);
    }
  }

  /**
   * Register event handlers
   */
  private registerEventHandlers(): void {
    if (!this.connection) return;

    // Receive regular notifications
    this.connection.on('NotificationReceived', (notification: Notification) => {
      this.notificationListeners.forEach((callback) => callback(notification));
    });

    // Receive alert
    this.connection.on('AlertReceived', (alert: AlertNotification) => {
      this.alertListeners.forEach((callback) => callback(alert));
      // Also trigger regular notification
      this.notificationListeners.forEach((callback) => callback(alert));
    });

    // Receive system message
    this.connection.on('SystemMessage', (message: SystemMessage) => {
      this.systemMessageListeners.forEach((callback) => callback(message));
      // Also trigger regular notification
      this.notificationListeners.forEach((callback) => callback(message));
    });

    // Receive log entry
    this.connection.on('LogEntry', (log: LogEntry) => {
      this.logEntryListeners.forEach((callback) => callback(log));
    });

    // Subscription success callback
    this.connection.on('Subscribed', (channel: string) => {
      console.log(`[SignalR] Successfully subscribed to: ${channel}`);
    });

    // Unsubscription callback
    this.connection.on('Unsubscribed', (channel: string) => {
      console.log(`[SignalR] Successfully unsubscribed from: ${channel}`);
    });

    // Notification acknowledgment callback
    this.connection.on('NotificationAcknowledged', (notificationId: string) => {
      console.log(`[SignalR] Notification acknowledged: ${notificationId}`);
    });
  }

  /**
   * Set connection state
   */
  private setConnectionState(state: ConnectionState): void {
    this.connectionState = state;
    this.connectionStateListeners.forEach((callback) => callback(state));
  }

  /**
   * Get current connection state
   */
  getConnectionState(): ConnectionState {
    return this.connectionState;
  }

  /**
   * Add notification listener
   */
  onNotification(callback: NotificationCallback): () => void {
    this.notificationListeners.add(callback);
    return () => this.notificationListeners.delete(callback);
  }

  /**
   * Add alert listener
   */
  onAlert(callback: AlertCallback): () => void {
    this.alertListeners.add(callback);
    return () => this.alertListeners.delete(callback);
  }

  /**
   * Add system message listener
   */
  onSystemMessage(callback: SystemMessageCallback): () => void {
    this.systemMessageListeners.add(callback);
    return () => this.systemMessageListeners.delete(callback);
  }

  /**
   * Add log entry listener
   */
  onLogEntry(callback: LogEntryCallback): () => void {
    this.logEntryListeners.add(callback);
    return () => this.logEntryListeners.delete(callback);
  }

  /**
   * Add connection state listener
   */
  onConnectionStateChange(callback: ConnectionStateCallback): () => void {
    this.connectionStateListeners.add(callback);
    return () => this.connectionStateListeners.delete(callback);
  }
}

// Singleton export
export const notificationHub = new NotificationHubService();
export default notificationHub;
