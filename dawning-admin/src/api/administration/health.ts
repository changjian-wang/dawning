import axios from '@/api/interceptor';

// 基本健康状态
export interface HealthStatus {
  status: string;
  timestamp: string;
  uptime: string;
}

// 详细健康检查结果
export interface DetailedHealthStatus extends HealthStatus {
  checks: {
    api: {
      status: string;
      responseTime?: string;
      error?: string;
    };
    database: {
      status: string;
      responseTime?: string;
      error?: string;
    };
    memory: {
      status: string;
      workingSet?: string;
      threshold?: string;
      error?: string;
    };
  };
}

// 性能指标
export interface Metrics {
  timestamp: string;
  uptime: string;
  memory: {
    workingSet: string;
    privateMemory: string;
    virtualMemory: string;
    gcMemory: string;
  };
  cpu: {
    totalProcessorTime: string;
    userProcessorTime: string;
  };
  threads: {
    count: number;
  };
  gc: {
    gen0Collections: number;
    gen1Collections: number;
    gen2Collections: number;
  };
}

// 服务状态
export interface ServiceStatus {
  name: string;
  url: string;
  status: 'healthy' | 'unhealthy' | 'unknown';
  responseTime?: number;
  lastChecked: string;
  error?: string;
}

export const healthApi = {
  // 获取基本健康状态
  async getHealth(): Promise<HealthStatus> {
    const response = await axios.get('/api/health');
    return response.data;
  },

  // 获取详细健康状态
  async getDetailedHealth(): Promise<DetailedHealthStatus> {
    const response = await axios.get('/api/health/detailed');
    return response.data;
  },

  // 获取就绪状态
  async getReady(): Promise<{ status: string; reason?: string }> {
    const response = await axios.get('/api/health/ready');
    return response.data;
  },

  // 获取存活状态
  async getLive(): Promise<{ status: string }> {
    const response = await axios.get('/api/health/live');
    return response.data;
  },

  // 获取性能指标
  async getMetrics(): Promise<Metrics> {
    const response = await axios.get('/api/health/metrics');
    return response.data;
  },

  // 检查指定 URL 的健康状态
  async checkService(name: string, url: string): Promise<ServiceStatus> {
    const startTime = Date.now();
    try {
      await axios.get(url, { timeout: 5000 });
      return {
        name,
        url,
        status: 'healthy',
        responseTime: Date.now() - startTime,
        lastChecked: new Date().toISOString(),
      };
    } catch (error: any) {
      return {
        name,
        url,
        status: 'unhealthy',
        responseTime: Date.now() - startTime,
        lastChecked: new Date().toISOString(),
        error: error.message,
      };
    }
  },
};

export default healthApi;
