import dayjs from 'dayjs';
import { Secret } from './secret';

export interface IClientSecret extends Secret {
  clientId: string;
}

/**
 * 优化点：
 * 1. 使用构造函数支持 Partial<IClientSecret> 初始化，减少冗余和硬编码。
 * 2. 默认值通过参数解构设置，初始化更灵活。
 * 3. 类型声明更清晰，方便扩展和维护。
 */
export class ClientSecret implements IClientSecret {
  clientId: string;
  id: string;
  type: string;
  description: string;
  value: string;
  expiration: string;
  created: string;

  constructor({
    clientId = '',
    id = '',
    type = 'Shared Secret',
    description = '',
    value = '',
    expiration = '',
    created = dayjs().format('YYYY-MM-DD'),
  }: Partial<IClientSecret> = {}) {
    this.clientId = clientId;
    this.id = id;
    this.type = type;
    this.description = description;
    this.value = value;
    this.expiration = expiration;
    this.created = created;
  }
}