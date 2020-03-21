﻿namespace BusinessApp.App
{
    using System.IO;

    public interface ISerializer
    {
        T Deserialize<T>(Stream serializationStream);
        void Serialize(Stream serializationStream, object graph);
    }
}