import axios from '@/api/interceptor';

export interface IProperty {
  id: string;
  key: string;
  value: string;
}

export interface IPropertyModel {
  key: string;
  value: string;
}
