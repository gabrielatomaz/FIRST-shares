﻿using FIRSTShares.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FIRSTShares.Models
{
    public class ComentarioModel
    {
        public string Conteudo { get; set; }
        public int IDPostagemPai { get; set; }
    }

    public class CurtirModel
    {
        public int PostID { get; set; }
        public bool Curtiu { get; set; }
    }
}
