using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Documentation;
using Oqtane.Interfaces;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Oqtane.Shared;
using System.IO;

namespace Oqtane.Modules.Admin.Files.Manager
{
    public class FileManager : ISearchable
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;
        private const string DocumentExtensions = ".txt,.htm,.html";

        public FileManager(IFolderRepository folderRepository, IFileRepository fileRepository)
        {
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
        }

        public async Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
            await Task.CompletedTask;

            var searchContents = new List<SearchContent>();

            var folders = _folderRepository.GetFolders(pageModule.Module.SiteId);
            foreach ( var folder in folders)
            {
                bool changed = false;
                bool removed = false;

                if (folder.ModifiedOn >= lastIndexedOn)
                {
                    changed = true;
                    removed = folder.IsDeleted.Value;
                }

                var files = _fileRepository.GetFiles(folder.FolderId);
                foreach (var file in files)
                {
                    if (file.ModifiedOn >= lastIndexedOn || changed)
                    {
                        var path = folder.Path + file.Name;

                        var body = "";
                        if (DocumentExtensions.Contains(Path.GetExtension(file.Name)))
                        {
                            // get the contents of the file
                            body = System.IO.File.ReadAllText(_fileRepository.GetFilePath(file));
                        }

                        var searchContent = new SearchContent
                        {
                            SiteId = folder.SiteId,
                            EntityName = EntityNames.File,
                            EntityId = file.FileId.ToString(),
                            Title = path,
                            Description = string.Empty,
                            Body = body,
                            Url = $"{Constants.FileUrl}{folder.Path}{file.Name}",
                            Permissions = $"{EntityNames.Folder}:{folder.FolderId}",
                            ContentModifiedBy = file.ModifiedBy,
                            ContentModifiedOn = file.ModifiedOn,
                            AdditionalContent = string.Empty,
                            CreatedOn = DateTime.UtcNow,
                            IsDeleted = (removed || file.IsDeleted.Value)
                        };
                        searchContents.Add(searchContent);
                    }

                }
            }

            return searchContents;
        }
    }
}
