<#
.SYNOPSIS
    Dawning Identity 数据库迁移工具

.DESCRIPTION
    自动化执行数据库迁移脚本，支持：
    - 按版本顺序执行迁移
    - 记录已执行的迁移
    - 支持回滚操作
    - 验证迁移状态

.PARAMETER Action
    执行的操作：migrate（执行迁移）, status（查看状态）, rollback（回滚）

.PARAMETER Version
    指定迁移版本（用于回滚操作）

.PARAMETER Server
    MySQL服务器地址（默认：localhost）

.PARAMETER Port
    MySQL端口（默认：3306）

.PARAMETER Database
    数据库名称（默认：dawning_identity）

.PARAMETER User
    数据库用户名（默认：root）

.EXAMPLE
    .\migrate-db.ps1 -Action status
    .\migrate-db.ps1 -Action migrate
    .\migrate-db.ps1 -Action rollback -Version V7
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("migrate", "status", "rollback", "init")]
    [string]$Action = "status",

    [Parameter(Mandatory=$false)]
    [string]$Version = "",

    [Parameter(Mandatory=$false)]
    [string]$Server = "localhost",

    [Parameter(Mandatory=$false)]
    [int]$Port = 3306,

    [Parameter(Mandatory=$false)]
    [string]$Database = "dawning_identity",

    [Parameter(Mandatory=$false)]
    [string]$User = "root"
)

$ErrorActionPreference = "Stop"

# 脚本目录
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$MigrationsPath = Join-Path $ScriptRoot "docs\sql\migrations"
$InitScriptPath = Join-Path $ScriptRoot "docs\sql\dawning_identity.sql"

# 颜色输出函数
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Write-Success { param([string]$Message) Write-ColorOutput "✓ $Message" "Green" }
function Write-Error { param([string]$Message) Write-ColorOutput "✗ $Message" "Red" }
function Write-Warning { param([string]$Message) Write-ColorOutput "! $Message" "Yellow" }
function Write-Info { param([string]$Message) Write-ColorOutput "→ $Message" "Cyan" }

# 获取MySQL密码
function Get-MySqlPassword {
    $securePassword = Read-Host "Enter MySQL password" -AsSecureString
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    }
}

# 执行MySQL命令
function Invoke-MySqlCommand {
    param(
        [string]$Query,
        [string]$Password,
        [switch]$ReturnResults
    )

    $mysqlArgs = @(
        "-h", $Server,
        "-P", $Port,
        "-u", $User,
        "-p$Password",
        "-D", $Database,
        "-N", "-B"
    )

    if ($ReturnResults) {
        $result = echo $Query | & mysql @mysqlArgs 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "MySQL Error: $result"
        }
        return $result
    } else {
        echo $Query | & mysql @mysqlArgs 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "MySQL command failed"
        }
    }
}

# 执行SQL文件
function Invoke-MySqlScript {
    param(
        [string]$ScriptPath,
        [string]$Password
    )

    $mysqlArgs = @(
        "-h", $Server,
        "-P", $Port,
        "-u", $User,
        "-p$Password",
        "-D", $Database
    )

    $result = Get-Content $ScriptPath | & mysql @mysqlArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Script execution failed: $result"
    }
    return $result
}

# 创建迁移历史表
function Initialize-MigrationTable {
    param([string]$Password)

    $createTableSql = @"
CREATE TABLE IF NOT EXISTS __migration_history (
    id INT AUTO_INCREMENT PRIMARY KEY,
    version VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    applied_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    checksum VARCHAR(64) NULL,
    status ENUM('applied', 'failed', 'rolled_back') DEFAULT 'applied'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
"@

    try {
        Invoke-MySqlCommand -Query $createTableSql -Password $Password
        Write-Success "Migration history table ready"
    }
    catch {
        Write-Error "Failed to create migration history table: $_"
        throw
    }
}

# 获取已应用的迁移
function Get-AppliedMigrations {
    param([string]$Password)

    $query = "SELECT version FROM __migration_history WHERE status = 'applied' ORDER BY id"
    try {
        $result = Invoke-MySqlCommand -Query $query -Password $Password -ReturnResults
        if ($result) {
            return $result -split "`n" | Where-Object { $_ }
        }
        return @()
    }
    catch {
        return @()
    }
}

# 获取待执行的迁移脚本
function Get-PendingMigrations {
    param([string[]]$AppliedVersions)

    $allMigrations = Get-ChildItem -Path $MigrationsPath -Filter "V*.sql" | 
        Sort-Object { [int]($_.Name -replace 'V(\d+).*', '$1') }

    $pending = @()
    foreach ($migration in $allMigrations) {
        $version = ($migration.Name -split '_')[0]  # V1, V2, etc.
        if ($version -notin $AppliedVersions) {
            $pending += @{
                Version = $version
                Name = $migration.Name
                Path = $migration.FullName
            }
        }
    }
    return $pending
}

# 计算文件校验和
function Get-FileChecksum {
    param([string]$FilePath)
    $hash = Get-FileHash -Path $FilePath -Algorithm SHA256
    return $hash.Hash.Substring(0, 16)
}

# 记录迁移
function Record-Migration {
    param(
        [string]$Version,
        [string]$Name,
        [string]$Checksum,
        [string]$Status,
        [string]$Password
    )

    $query = "INSERT INTO __migration_history (version, name, checksum, status) VALUES ('$Version', '$Name', '$Checksum', '$Status')"
    Invoke-MySqlCommand -Query $query -Password $Password
}

# 更新迁移状态
function Update-MigrationStatus {
    param(
        [string]$Version,
        [string]$Status,
        [string]$Password
    )

    $query = "UPDATE __migration_history SET status = '$Status' WHERE version = '$Version'"
    Invoke-MySqlCommand -Query $query -Password $Password
}

# 显示迁移状态
function Show-MigrationStatus {
    param([string]$Password)

    Write-Host ""
    Write-ColorOutput "=== Dawning Identity 数据库迁移状态 ===" "Cyan"
    Write-Host ""

    # 检查迁移表
    Initialize-MigrationTable -Password $Password

    # 获取已应用的迁移
    $applied = Get-AppliedMigrations -Password $Password
    Write-Info "已应用的迁移: $($applied.Count)"

    if ($applied.Count -gt 0) {
        foreach ($v in $applied) {
            Write-Success "  $v"
        }
    }

    # 获取待执行的迁移
    $pending = Get-PendingMigrations -AppliedVersions $applied
    Write-Host ""
    Write-Info "待执行的迁移: $($pending.Count)"

    if ($pending.Count -gt 0) {
        foreach ($m in $pending) {
            Write-Warning "  $($m.Version): $($m.Name)"
        }
    } else {
        Write-Success "  数据库已是最新版本"
    }

    Write-Host ""
}

# 执行迁移
function Invoke-Migration {
    param([string]$Password)

    Write-Host ""
    Write-ColorOutput "=== 执行数据库迁移 ===" "Cyan"
    Write-Host ""

    # 初始化迁移表
    Initialize-MigrationTable -Password $Password

    # 获取待执行迁移
    $applied = Get-AppliedMigrations -Password $Password
    $pending = Get-PendingMigrations -AppliedVersions $applied

    if ($pending.Count -eq 0) {
        Write-Success "数据库已是最新版本，无需迁移"
        return
    }

    Write-Info "发现 $($pending.Count) 个待执行迁移"
    Write-Host ""

    foreach ($migration in $pending) {
        Write-Info "正在执行: $($migration.Name)"

        try {
            $checksum = Get-FileChecksum -FilePath $migration.Path

            # 执行迁移脚本
            $output = Invoke-MySqlScript -ScriptPath $migration.Path -Password $Password

            # 记录成功
            Record-Migration -Version $migration.Version -Name $migration.Name -Checksum $checksum -Status "applied" -Password $Password

            Write-Success "完成: $($migration.Version)"
            if ($output) {
                Write-Host "  $output" -ForegroundColor Gray
            }
        }
        catch {
            Write-Error "失败: $($migration.Version) - $_"

            # 记录失败
            try {
                Record-Migration -Version $migration.Version -Name $migration.Name -Checksum "" -Status "failed" -Password $Password
            }
            catch { }

            throw "迁移失败，已停止"
        }
    }

    Write-Host ""
    Write-Success "所有迁移已完成！"
}

# 回滚迁移（标记为已回滚，实际回滚需手动操作）
function Invoke-Rollback {
    param(
        [string]$TargetVersion,
        [string]$Password
    )

    if (-not $TargetVersion) {
        Write-Error "请使用 -Version 参数指定要回滚到的版本"
        return
    }

    Write-Host ""
    Write-ColorOutput "=== 回滚迁移到 $TargetVersion ===" "Yellow"
    Write-Host ""

    # 更新状态为已回滚
    Update-MigrationStatus -Version $TargetVersion -Status "rolled_back" -Password $Password

    Write-Warning "已将 $TargetVersion 标记为已回滚"
    Write-Warning "请手动执行对应的回滚SQL语句（通常在迁移脚本注释中）"
}

# 主程序
function Main {
    Write-Host ""
    Write-ColorOutput "Dawning Identity 数据库迁移工具 v1.0" "Magenta"
    Write-ColorOutput "服务器: $Server`:$Port  数据库: $Database" "Gray"
    Write-Host ""

    # 检查MySQL客户端
    if (-not (Get-Command mysql -ErrorAction SilentlyContinue)) {
        Write-Error "未找到 MySQL 客户端，请确保 mysql 命令可用"
        exit 1
    }

    # 检查迁移目录
    if (-not (Test-Path $MigrationsPath)) {
        Write-Error "迁移目录不存在: $MigrationsPath"
        exit 1
    }

    # 获取密码
    $password = Get-MySqlPassword

    try {
        switch ($Action) {
            "status" {
                Show-MigrationStatus -Password $password
            }
            "migrate" {
                Invoke-Migration -Password $password
            }
            "rollback" {
                Invoke-Rollback -TargetVersion $Version -Password $password
            }
            "init" {
                Write-Info "初始化数据库..."
                if (Test-Path $InitScriptPath) {
                    Invoke-MySqlScript -ScriptPath $InitScriptPath -Password $password
                    Write-Success "数据库初始化完成"
                } else {
                    Write-Error "初始化脚本不存在: $InitScriptPath"
                }
            }
        }
    }
    catch {
        Write-Error $_
        exit 1
    }
}

# 执行
Main
