import UserClaim from './user-claim';

export interface IIdentityResourceClaim {
  /**
   * 主键ID
   */
  id: string;

  /**
   * 声明类型
   */
  type: string;

  /**
   * 身份资源ID
   */
  identityResourceId: string;
}

export class IdentityResourceClaim
  implements IIdentityResourceClaim, UserClaim
{
  id = '';
  type = '';
  identityResourceId = '';

  constructor(data?: Partial<IIdentityResourceClaim>) {
    if (data) {
      Object.assign(this, data);
    }
  }
}

export default IdentityResourceClaim;
