export interface IUserClaim {
  /**
   * Primary key ID
   */
  id: string;

  /**
   * Claim type
   */
  type: string;
}

export abstract class UserClaim implements IUserClaim {
  id = '';
  type = '';

  constructor(data?: Partial<IUserClaim>) {
    if (data) {
      Object.assign(this, data);
    }
  }
}

export default UserClaim;
