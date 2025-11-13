using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities
{
	[Table("user")]
	public class User
	{
		[ExplicitKey]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Column("name")]
		public string? Name { get; set; }

		[Column("age")]
		public int Age { get; set; }

		[Column("created_on")]
		public DateTime? CreatedOn { get; set; } = DateTime.Now;

		[Column("updated_on")]
		[IgnoreUpdate]
        public DateTime? UpdateedOn { get; set; }
    }
}

