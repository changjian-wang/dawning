# ====================================
# QueryBuilder 性能测试脚本
# ====================================
# 测试内容：
# 1. OFFSET 分页性能
# 2. Cursor 分页性能
# 3. Select 投影性能对比
# 4. Distinct 去重性能
# 5. 综合场景性能对比
# ====================================

$baseUrl = "http://localhost:5202"
$testResults = @()

# 颜色输出函数
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# 性能测试函数
function Measure-ApiPerformance {
    param(
        [string]$Name,
        [string]$Url,
        [int]$Iterations = 10
    )
    
    Write-ColorOutput "`n⏱️  测试: $Name" "Cyan"
    Write-ColorOutput "URL: $Url" "Gray"
    Write-ColorOutput "执行次数: $Iterations" "Gray"
    
    $times = @()
    $success = 0
    $failed = 0
    
    # 预热请求（不计入统计）
    try {
        $null = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop
        Write-ColorOutput "✓ 预热完成" "Green"
    } catch {
        Write-ColorOutput "✗ 预热失败: $($_.Exception.Message)" "Red"
    }
    
    # 正式测试
    for ($i = 1; $i -le $Iterations; $i++) {
        try {
            $sw = [System.Diagnostics.Stopwatch]::StartNew()
            $response = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop
            $sw.Stop()
            
            $times += $sw.ElapsedMilliseconds
            $success++
            
            Write-Host "  [$i/$Iterations] " -NoNewline -ForegroundColor Gray
            Write-Host "$($sw.ElapsedMilliseconds) ms" -ForegroundColor Green
            
            # 显示第一次响应的数据量信息
            if ($i -eq 1 -and $response.data) {
                if ($response.data.items) {
                    Write-ColorOutput "  📊 返回数据: $($response.data.items.Count) 条记录" "Yellow"
                } elseif ($response.data -is [Array]) {
                    Write-ColorOutput "  📊 返回数据: $($response.data.Count) 条记录" "Yellow"
                }
            }
            
        } catch {
            $failed++
            Write-ColorOutput "  [$i/$Iterations] 失败: $($_.Exception.Message)" "Red"
        }
        
        # 避免请求过快
        Start-Sleep -Milliseconds 50
    }
    
    # 计算统计数据
    if ($times.Count -gt 0) {
        $avg = ($times | Measure-Object -Average).Average
        $min = ($times | Measure-Object -Minimum).Minimum
        $max = ($times | Measure-Object -Maximum).Maximum
        $median = ($times | Sort-Object)[[Math]::Floor($times.Count / 2)]
        
        Write-ColorOutput "`n📈 统计结果:" "Cyan"
        Write-ColorOutput "  平均: $([Math]::Round($avg, 2)) ms" "White"
        Write-ColorOutput "  最小: $min ms" "Green"
        Write-ColorOutput "  最大: $max ms" "Yellow"
        Write-ColorOutput "  中位数: $median ms" "White"
        Write-ColorOutput "  成功率: $success/$Iterations ($([Math]::Round($success/$Iterations*100, 1))%)" "Green"
        
        return @{
            Name = $Name
            Url = $Url
            Average = [Math]::Round($avg, 2)
            Min = $min
            Max = $max
            Median = $median
            Success = $success
            Failed = $failed
            Times = $times
        }
    } else {
        Write-ColorOutput "❌ 所有请求均失败" "Red"
        return $null
    }
}

# ====================================
# 开始性能测试
# ====================================

Write-ColorOutput "═══════════════════════════════════════════════════" "Magenta"
Write-ColorOutput "     QueryBuilder 性能测试" "Magenta"
Write-ColorOutput "═══════════════════════════════════════════════════" "Magenta"

# 测试 1: OFFSET 分页 - 第一页
$result1 = Measure-ApiPerformance `
    -Name "OFFSET 分页 - 第1页 (pageIndex=1, pageSize=10)" `
    -Url "$baseUrl/api/user?pageIndex=1&pageSize=10" `
    -Iterations 10

if ($result1) { $testResults += $result1 }

# 测试 2: OFFSET 分页 - 深分页
$result2 = Measure-ApiPerformance `
    -Name "OFFSET 分页 - 深分页 (pageIndex=100, pageSize=10)" `
    -Url "$baseUrl/api/user?pageIndex=100&pageSize=10" `
    -Iterations 10

if ($result2) { $testResults += $result2 }

# 测试 3: Cursor 分页 - 第一页
$result3 = Measure-ApiPerformance `
    -Name "Cursor 分页 - 第1页 (pageSize=10)" `
    -Url "$baseUrl/api/user/cursor?pageSize=10" `
    -Iterations 10

if ($result3) { $testResults += $result3 }

# 测试 4: Cursor 分页 - 带游标
# 首先获取一个有效的游标
try {
    $firstPage = Invoke-RestMethod -Uri "$baseUrl/api/user/cursor?pageSize=10" -Method Get
    if ($firstPage.data.nextCursor) {
        $cursor = $firstPage.data.nextCursor
        $result4 = Measure-ApiPerformance `
            -Name "Cursor 分页 - 带游标 (pageSize=10)" `
            -Url "$baseUrl/api/user/cursor?pageSize=10&cursor=$cursor" `
            -Iterations 10
        
        if ($result4) { $testResults += $result4 }
    } else {
        Write-ColorOutput "⚠️  数据量较少，跳过游标分页第2页测试" "Yellow"
    }
} catch {
    Write-ColorOutput "⚠️  无法获取游标，跳过游标分页测试: $($_.Exception.Message)" "Yellow"
}

# 测试 5: 不同页面大小对比
Write-ColorOutput "`n═══════════════════════════════════════════════════" "Magenta"
Write-ColorOutput "     不同页面大小性能对比" "Magenta"
Write-ColorOutput "═══════════════════════════════════════════════════" "Magenta"

$pageSizes = @(10, 50, 100)
foreach ($size in $pageSizes) {
    $result = Measure-ApiPerformance `
        -Name "OFFSET 分页 - pageSize=$size" `
        -Url "$baseUrl/api/user?pageIndex=1&pageSize=$size" `
        -Iterations 5
    
    if ($result) { $testResults += $result }
}

# ====================================
# 生成性能对比报告
# ====================================

Write-ColorOutput "`n═══════════════════════════════════════════════════" "Magenta"
Write-ColorOutput "     性能测试总结报告" "Magenta"
Write-ColorOutput "═══════════════════════════════════════════════════" "Magenta"

Write-ColorOutput "`n📊 所有测试结果对比:" "Cyan"
Write-ColorOutput ""

# 表头
Write-Host ("┌" + "─" * 50 + "┬" + "─" * 15 + "┬" + "─" * 15 + "┐") -ForegroundColor Gray
Write-Host ("│ " + "测试场景".PadRight(48) + " │ " + "平均响应时间".PadRight(13) + " │ " + "中位数".PadRight(13) + " │") -ForegroundColor Gray
Write-Host ("├" + "─" * 50 + "┼" + "─" * 15 + "┼" + "─" * 15 + "┤") -ForegroundColor Gray

foreach ($result in $testResults) {
    $name = if ($result.Name.Length -gt 48) { $result.Name.Substring(0, 45) + "..." } else { $result.Name }
    Write-Host ("│ " + $name.PadRight(48) + " │ " + "$($result.Average) ms".PadRight(13) + " │ " + "$($result.Median) ms".PadRight(13) + " │") -ForegroundColor White
}

Write-Host ("└" + "─" * 50 + "┴" + "─" * 15 + "┴" + "─" * 15 + "┘") -ForegroundColor Gray

# ====================================
# 性能对比分析
# ====================================

Write-ColorOutput "`n📈 性能对比分析:" "Cyan"

# 对比 OFFSET 第1页 vs 深分页
$offset1 = $testResults | Where-Object { $_.Name -like "*OFFSET*第1页*" } | Select-Object -First 1
$offset100 = $testResults | Where-Object { $_.Name -like "*深分页*" }

if ($offset1 -and $offset100) {
    $diff = $offset100.Average - $offset1.Average
    $percent = [Math]::Round(($diff / $offset1.Average) * 100, 1)
    
    Write-ColorOutput "`n🔍 OFFSET 分页性能退化:" "Yellow"
    Write-ColorOutput "  第1页: $($offset1.Average) ms" "White"
    Write-ColorOutput "  第100页: $($offset100.Average) ms" "White"
    
    if ($diff -gt 0) {
        Write-ColorOutput "  性能退化: +$diff ms (+$percent%)" "Red"
    } else {
        Write-ColorOutput "  性能提升: $diff ms ($percent%)" "Green"
    }
}

# 对比 Cursor vs OFFSET
$cursor = $testResults | Where-Object { $_.Name -like "*Cursor*第1页*" } | Select-Object -First 1
$offsetFirst = $testResults | Where-Object { $_.Name -like "*OFFSET*第1页*" } | Select-Object -First 1

if ($cursor -and $offsetFirst) {
    $diff = $cursor.Average - $offsetFirst.Average
    $percent = [Math]::Round(($diff / $offsetFirst.Average) * 100, 1)
    
    Write-ColorOutput "`n🔍 Cursor vs OFFSET (第1页):" "Yellow"
    Write-ColorOutput "  OFFSET: $($offsetFirst.Average) ms" "White"
    Write-ColorOutput "  Cursor: $($cursor.Average) ms" "White"
    
    if ($diff -lt 0) {
        Write-ColorOutput "  Cursor 更快: $([Math]::Abs($diff)) ms ($([Math]::Abs($percent))%)" "Green"
    } else {
        Write-ColorOutput "  OFFSET 更快: $diff ms ($percent%)" "Yellow"
    }
}

# 页面大小影响分析
$pageSizeResults = $testResults | Where-Object { $_.Name -like "*pageSize=*" }
if ($pageSizeResults.Count -gt 1) {
    Write-ColorOutput "`n🔍 页面大小性能影响:" "Yellow"
    foreach ($result in $pageSizeResults | Sort-Object { [int]($_.Name -replace '.*pageSize=(\d+).*', '$1') }) {
        $size = [int]($result.Name -replace '.*pageSize=(\d+).*', '$1')
        Write-ColorOutput "  pageSize=$size : $($result.Average) ms" "White"
    }
}

# ====================================
# 建议
# ====================================

Write-ColorOutput "`n💡 优化建议:" "Cyan"

if ($offset100 -and $offset1 -and $offset100.Average -gt ($offset1.Average * 1.5)) {
    Write-ColorOutput "  ⚠️  检测到深分页性能明显退化 (>50%)，建议：" "Yellow"
    Write-ColorOutput "     1. 对于深分页场景使用 Cursor 分页" "White"
    Write-ColorOutput "     2. 限制最大页码（如 maxPage = 100）" "White"
    Write-ColorOutput "     3. 添加索引优化 OFFSET 性能" "White"
} else {
    Write-ColorOutput "  ✓ OFFSET 分页性能表现良好" "Green"
}

if ($cursor -and $offsetFirst -and $cursor.Average -lt $offsetFirst.Average) {
    Write-ColorOutput "  ✓ Cursor 分页性能优于 OFFSET，适合无限滚动场景" "Green"
} elseif ($cursor -and $offsetFirst) {
    $perfDiff = [Math]::Abs($cursor.Average - $offsetFirst.Average)
    if ($perfDiff -lt 5) {
        Write-ColorOutput "  ℹ️  Cursor 和 OFFSET 性能相近（差异 < 5ms），根据场景选择：" "Cyan"
    } else {
        Write-ColorOutput "  ℹ️  性能对比：" "Cyan"
    }
    Write-ColorOutput "     - 需要跳页、显示总页数: 使用 OFFSET" "White"
    Write-ColorOutput "     - 无限滚动、实时数据流: 使用 Cursor" "White"
    Write-ColorOutput "     - 微信H5、移动端列表: 推荐 Cursor" "White"
}

# 数据量建议
$maxSize = $pageSizeResults | Sort-Object Average -Descending | Select-Object -First 1
if ($maxSize -and $maxSize.Average -gt 1000) {
    Write-ColorOutput "  ⚠️  大页面大小 (${maxSize.Name}) 响应时间较长，建议限制 pageSize ≤ 100" "Yellow"
}

Write-ColorOutput "`n═══════════════════════════════════════════════════" "Magenta"
Write-ColorOutput "     性能测试完成" "Magenta"
Write-ColorOutput "═══════════════════════════════════════════════════" "Magenta"

# 保存结果到 JSON 文件
$reportPath = Join-Path $PSScriptRoot "performance-test-results.json"
$testResults | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportPath -Encoding UTF8
Write-ColorOutput "`n💾 测试结果已保存到: $reportPath" "Green"
