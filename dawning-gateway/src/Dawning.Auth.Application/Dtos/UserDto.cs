using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dawning.Auth.Application.Dtos
{
	public class UserDto
	{
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public int Age { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdateedOn { get; set; }
    }
}

