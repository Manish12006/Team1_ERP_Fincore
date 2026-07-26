using AutoMapper;
using Fincore.Application.DTO.MasterTable;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Fincore.Domain.Models;

namespace Fincore.Application.AutoMapper
{
    public class DocumentMappingProfile : Profile
    {
        public DocumentMappingProfile() 
        {
            CreateMap<Document, DocumentDto>();

            CreateMap<CreateDocumentDto, Document>()
                .ForMember(x => x.FileName, opt => opt.Ignore())
                .ForMember(x => x.FileType, opt => opt.Ignore())
                .ForMember(x => x.FilePath, opt => opt.Ignore());

            CreateMap<UpdateDocumentDto, Document>();


            CreateMap<DocumentType, DocumentTypeDto>();
            CreateMap<CreateDocumentTypeDto, DocumentType>();
            CreateMap<UpdateDocumentTypeDto, DocumentType>();


            CreateMap<MasterType, MasterTypeDto>();
            CreateMap<CreateMasterTypeDto, MasterType>();
            CreateMap<UpdateMasterTypeDto, MasterType>();



        }




    }
}
