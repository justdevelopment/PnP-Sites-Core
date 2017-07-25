﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml.Resolvers
{
    internal class FoldersFromModelToSchemaTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;
        public bool CustomCollectionResolver => false;

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            Array result = null;

            Object modelSource = source as Model.ListInstance;
            if (modelSource == null)
            {
                modelSource = source as Model.Folder;
            }

            if (modelSource != null)
            {
                Model.FolderCollection sourceFolders= modelSource.GetPublicInstancePropertyValue("Folders") as Model.FolderCollection;
                if (sourceFolders != null)
                {
                    var folderTypeName = $"{PnPSerializationScope.Current?.BaseSchemaNamespace}.Folder, {PnPSerializationScope.Current?.BaseSchemaAssemblyName}";
                    var folderType = Type.GetType(folderTypeName, true);

                    result = Array.CreateInstance(folderType, sourceFolders.Count);

                    resolvers = new Dictionary<string, IResolver>();
                    resolvers.Add($"{folderType}.Folder1", new FoldersFromModelToSchemaTypeResolver());
                    resolvers.Add($"{folderType}.Security", new SecurityFromModelToSchemaTypeResolver());

                    for (Int32 c = 0; c < sourceFolders.Count; c++)
                    {
                        var targetFolderItem = Activator.CreateInstance(folderType);
                        PnPObjectsMapper.MapProperties(sourceFolders[c], targetFolderItem, resolvers, recursive);

                        result.SetValue(targetFolderItem, c);
                    }
                }
            }

            return (result);
        }
    }
}
