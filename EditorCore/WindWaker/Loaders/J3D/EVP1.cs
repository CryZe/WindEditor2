﻿using GameFormatReader.Common;
using OpenTK;
using System.Collections.Generic;

namespace WEditor.WindWaker.Loaders
{
    public partial class J3DLoader
    {
        private sealed class Envelopes
        {
            public List<ushort> numBonesAffecting;
            public List<ushort> indexRemap;
            public List<float> weights;
            public List<Matrix3x4> inverseBindPose;

            public Envelopes()
            {
                numBonesAffecting = new List<ushort>();
                indexRemap = new List<ushort>();
                weights = new List<float>();
                inverseBindPose = new List<Matrix3x4>();
            }
        }

        private static Envelopes LoadEVP1FromStream(EndianBinaryReader reader, long chunkStart)
        {
            Envelopes envelopes = new Envelopes();
            ushort numEnvelopes = reader.ReadUInt16();
            reader.ReadUInt16(); // Padding

            // numEnvelope many uint8 - each one describes how many bones belong to this index.
            uint boneCountOffset = reader.ReadUInt32();
            // "sum over all bytes in boneCountOffset many shorts (index into some joint stuff? into matrix table?)"
            uint indexDataOffset = reader.ReadUInt32();
            // Bone Weights (as many floats here as there are ushorts at indexDataOffset)
            uint weightOffset = reader.ReadUInt32();
            // Matrix Table (3x4 float array) - Inverse Bind Pose
            uint boneMatrixOffset = reader.ReadUInt32();


            // - Is this the number of bones which influence the vert?
            reader.BaseStream.Position = chunkStart + boneCountOffset;
            for (int b = 0; b < numEnvelopes; b++)
                envelopes.numBonesAffecting.Add(reader.ReadByte());

            // ???
            reader.BaseStream.Position = chunkStart + indexDataOffset;
            for (int m = 0; m < envelopes.numBonesAffecting.Count; m++)
            {
                for (int j = 0; j < envelopes.numBonesAffecting[m]; j++)
                {
                    envelopes.indexRemap.Add(reader.ReadUInt16());
                }
            }

            // Bone Weights
            reader.BaseStream.Position = chunkStart + weightOffset;
            for (int w = 0; w < envelopes.numBonesAffecting.Count; w++)
            {
                for (int j = 0; j < envelopes.numBonesAffecting[w]; j++)
                {
                    envelopes.weights.Add(reader.ReadSingle());
                }
            }

            // Inverse Bind Pose Matrices
            reader.BaseStream.Position = chunkStart + boneMatrixOffset;
            for (int w = 0; w < numEnvelopes; w++)
            {
                Matrix3x4 matrix = new Matrix3x4();
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 4; k++)
                        matrix[j, k] = reader.ReadSingle();
                }

                envelopes.inverseBindPose.Add(matrix);
            }

            return envelopes;
        }
    }
}
