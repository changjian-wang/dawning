export interface IUserClaim {
  /**
   * 主键ID
   */
  id: string;

  /**
   * 声明类型
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
