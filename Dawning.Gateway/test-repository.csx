// 临时测试 - 验证UserRepository.GetPagedListAsync
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Repository.Administration;

var connectionString = "data source=localhost; database=dawning_identity; uid=aluneth; pwd=123456";
var context = new DbContext(connectionString);
var repository = new UserRepository(context);

Console.WriteLine("测试GetPagedListAsync...");

try
{
    var model = new UserModel
    {
        IsActive = true,
        IncludeDeleted = false
    };

    var result = await repository.GetPagedListAsync(model, 1, 10);

    Console.WriteLine($"✅ 成功! 共{result.TotalCount}条记录");
    foreach (var user in result.Items)
    {
        Console.WriteLine($"  - {user.Username} ({user.Email})");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 错误: {ex.Message}");
    Console.WriteLine($"堆栈: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"内部异常: {ex.InnerException.Message}");
    }
}
finally
{
    context.Dispose();
}
