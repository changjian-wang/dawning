import { IApiResourceClaim } from "./api-resource-claim";
import { IApiResourceProperty } from "./api-resource-property";
import { IApiResourceScope } from "./api-resource-scope";
import { IApiResourceSecret } from "./api-resource-secret";

export interface IApiResource { 
    id: string;
    enabled: boolean;
    name: string;
    displayName: string;
    description: string;
    allowedAccessTokenSigningAlgorithms: string;
    showInDiscoveryDocument: boolean;
    secrets: IApiResourceSecret[];
    scopes: IApiResourceScope[];
    claims: IApiResourceClaim[];
    properties: IApiResourceProperty[];
    nonEditable: boolean;
}

/**
 * 优化说明:
 * 1. 提供构造函数，支持初始化对象属性。
 * 2. 支持传入部分属性，避免手动赋值全部属性。
 * 3. 默认值通过参数解构设置，更灵活。
 * 4. 增加类型安全，方便后续拓展和维护。
 */
export class ApiResource implements IApiResource {
    id: string;
    enabled: boolean;
    name: string;
    displayName: string;
    description: string;
    allowedAccessTokenSigningAlgorithms: string;
    showInDiscoveryDocument: boolean;
    secrets: IApiResourceSecret[];
    scopes: IApiResourceScope[];
    claims: IApiResourceClaim[];
    properties: IApiResourceProperty[];
    nonEditable: boolean;

    constructor({
        id = '',
        enabled = false,
        name = '',
        displayName = '',
        description = '',
        allowedAccessTokenSigningAlgorithms = '',
        showInDiscoveryDocument = true,
        secrets = [],
        scopes = [],
        claims = [],
        properties = [],
        nonEditable = false,
    }: Partial<IApiResource> = {}) {
        this.id = id;
        this.enabled = enabled;
        this.name = name;
        this.displayName = displayName;
        this.description = description;
        this.allowedAccessTokenSigningAlgorithms = allowedAccessTokenSigningAlgorithms;
        this.showInDiscoveryDocument = showInDiscoveryDocument;
        this.secrets = secrets;
        this.scopes = scopes;
        this.claims = claims;
        this.properties = properties;
        this.nonEditable = nonEditable;
    }
}