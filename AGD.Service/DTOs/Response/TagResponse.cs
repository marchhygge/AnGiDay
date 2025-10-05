using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Response
{
    public class TagResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;
    }
}
