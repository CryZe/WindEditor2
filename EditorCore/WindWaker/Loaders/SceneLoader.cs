﻿using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Common.Maps;
using WEditor.FileSystem;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders
{
    public class SceneLoader
    {
        public T LoadFromArchive<T>(WWorld world, ZArchive archive) where T : Scene, new()
        {
            if (world == null)
                throw new ArgumentNullException("world must not be null!");
            if (archive == null)
                throw new ArgumentNullException("archive must not be null!");

            T scene = new T();

            // Wind Waker only supports one-level deep of folders (ie: root/<folders>/<files>) so
            // we don't load this recursively.
            foreach (var vFolder in archive.Contents.Children)
            {
                // Skip loose files alongside the top level folders inside the root.
                if (vFolder.Type != NodeType.Directory)
                    continue;

                VirtualFilesystemDirectory folder = (VirtualFilesystemDirectory)vFolder;
                foreach (var vFile in folder.Children)
                {
                    // Skip folder inside of these folders, the game doesn't use them and it's probably a mis-placed/broken file.
                    if (vFile.Type != NodeType.File)
                        continue;

                    VirtualFilesystemFile file = (VirtualFilesystemFile)vFile;

                    switch (folder.Name.ToLower())
                    {
                        /* Room and Stage Entity Data */
                        case "dzr":
                        case "dzs":
                            LoadEntityData(file, scene, world);
                            break;
                        /* 3D Model Formats */
                        case "bmd":
                        case "bdl":
                            LoadModelData(file, scene, world);
                            break;
                        /* Map Collision Format */
                        case "dzb":
                        /* Event List */
                        case "dat":
                        /* External Textures (skybox?) */
                        case "bti":
                        /* Bone Animation */
                        case "bck":
                        /* TEV Register Animation */
                        case "brk":
                        /* Texture Animation */
                        case "btk":
                        /* Alternate Materials (MAT3 Chunk from BMD/BDL) */
                        case "bmt":
                            break;
                        default:
                            WLog.Warning(LogCategory.ArchiveLoading, archive, "Unknown folder type \"{0}\" found in Archive \"{1}\"", folder.Name, archive.Name);
                            break;
                    }
                }

            }

            return scene;
        }

        private void LoadEntityData(VirtualFilesystemFile file, Scene scene, WWorld world)
        {
            WLog.Info(LogCategory.ArchiveLoading, scene, "Loading DZR/DZS (Room/Stage Entity Data) {0}{1}...", file.Name, file.Extension);

            // We're going to load this DZS file into a generic list of all things contained by the DZS/DZR file, and then we'll manually pick through
            // chunks who's info we care about. Some objects will get stored on the Stage and discarded, while others will get converted into intermediate
            // formats.
            using(EndianBinaryReader reader = new EndianBinaryReader(file.Data.GetData(), Endian.Big))
            {
                MapEntityLoader entityLoader = new MapEntityLoader();
                scene.Entities = entityLoader.LoadFromStream(reader);
            }

            WLog.Info(LogCategory.ArchiveLoading, scene, "Loaded {0}{1}.", file.Name, file.Extension);
        }

        public void PostProcessEntityData(Map map)
        {
            MapEntityLoader entityLoader = new MapEntityLoader();
            if(map.NewStage != null)
                entityLoader.PostProcess(map.NewStage, map);

            foreach (var room in map.NewRooms)
                entityLoader.PostProcess(room, map);
        }

        private void LoadModelData(VirtualFilesystemFile file, Scene scene, WWorld world)
        {
            WLog.Info(LogCategory.ArchiveLoading, scene, "Loading BMD/BDL (3D Model) {0}{1}...", file.Name, file.Extension);
            
            using(EndianBinaryReader reader = new EndianBinaryReader(file.Data.GetData(), Endian.Big))
            {
                J3DLoader modelLoader = new J3DLoader();
                Mesh model = modelLoader.LoadFromStream(reader);
                world.RenderSystem.RegisterMesh(model);

                scene.Meshes.Add(model);
            }


            WLog.Info(LogCategory.ArchiveLoading, scene, "Loaded {0}{1}.", file.Name, file.Extension);
        }

    }
}