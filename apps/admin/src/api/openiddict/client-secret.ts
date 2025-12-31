import dayjs from 'dayjs';
import { Secret } from './secret';

export interface IClientSecret extends Secret {
  clientId: string;
}

/**
 * Optimizations:
 * 1. Use constructor supporting Partial<IClientSecret> initialization to reduce redundancy and hard-coding.
 * 2. Default values are set via parameter destructuring for more flexible initialization.
 * 3. Type declarations are clearer, easier to extend and maintain.
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
