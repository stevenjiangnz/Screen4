using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Screen.Entity;

namespace Screen.ProcessFunction.asxetf
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ScanResultEntity, ScanResultBullEntity>();
            CreateMap<ScanResultEntity, ScanResultBearEntity>();
        }
    }

}
