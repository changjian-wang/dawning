import { IProperty } from './property';

export interface IIdentityResourceProperty extends IProperty {
  /**
   * Primary key ID
   */
  id: string;

  /**
   * Claim type
   */
  type: string;

  /**
   * Identity resource ID
   */
  identityResourceId: string;
}

export class IdentityResourceProperty implements IIdentityResourceProperty {
  id = '';
  type = '';
  key = '';
  value = '';
  identityResourceId = '';

  constructor(data?: Partial<IIdentityResourceProperty>) {
    if (data) {
      Object.assign(this, data);
    }
  }

  /**
   * Sets the key-value pair for the property.
   * @param key - The key of the property.
   * @param value - The value of the property.
   */
  setProperty(key: string, value: string): void {
    this.key = key;
    this.value = value;
  }

  /**
   * Retrieves the key-value pair as an object.
   * @returns An object containing the key and value.
   */
  getProperty(): { key: string; value: string } {
    return { key: this.key, value: this.value };
  }
}

export default IdentityResourceProperty;
