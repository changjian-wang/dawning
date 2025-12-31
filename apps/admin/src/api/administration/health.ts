import axios from '@/api/interceptor';

// Basic health status
export interface HealthStatus {
  status: string;
  timestamp: string;
  uptime: string;
}

// Detailed health check result
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

// Performance metrics
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

// Service status
export interface ServiceStatus {
  name: string;
  url: string;
  status: 'healthy' | 'unhealthy' | 'unknown';
  responseTime?: number;
  lastChecked: string;
  error?: string;
}

export const healthApi = {
  // Get basic health status
  async getHealth(): Promise<HealthStatus> {
    const response = await axios.get('/api/health');
    return response.data;
  },

  // Get detailed health status
  async getDetailedHealth(): Promise<DetailedHealthStatus> {
    const response = await axios.get('/api/health/detailed');
    return response.data;
  },

  // Get ready status
  async getReady(): Promise<{ status: string; reason?: string }> {
    const response = await axios.get('/api/health/ready');
    return response.data;
  },

  // Get liveness status
  async getLive(): Promise<{ status: string }> {
    const response = await axios.get('/api/health/live');
    return response.data;
  },

  // Get performance metrics
  async getMetrics(): Promise<Metrics> {
    const response = await axios.get('/api/health/metrics');
    return response.data;
  },

  // Check health status of specified URL
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
