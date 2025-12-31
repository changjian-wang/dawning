import UserClaim from './user-claim';

export interface IIdentityResourceClaim {
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
