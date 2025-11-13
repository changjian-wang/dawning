export interface Secret {
  id: string;
  description: string;
  value: string;
  expiration: string;
  type?: string;
  created: string;
}
