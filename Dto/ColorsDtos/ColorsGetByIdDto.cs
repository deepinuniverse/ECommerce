﻿using Ecommerce.Entities;
using System.Text.Json.Serialization;

namespace Dto.ColorsDtos
{
    public class ColorsGetByIdDto
    {
        public string Name { get; set; }

        public string ColorCode { get; set; }

        //ForeignKey
        [JsonIgnore] public ICollection<Price>? Prices { get; set; }
    }
}
