// EntityTestBase.cs
// By: Adam Renaud
// Created on: 2019-01-13

// System Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

// Test Using Statements
using Xunit;
using Xunit.Abstractions;

// Internal Using Statements
using DxfLibrary.Entities;
using DxfLibrary.Geometry;

namespace DxfLibrary.Tests.Entities
{
    /// <summary>
    /// Base class for all entities. Contains the functionality 
    /// for a virual file using a memory stream. A string can be written to
    /// the stream and then passed to the dxf parser to simulate a file.
    /// </summary>
    public class EntityTestBase : TestBase, IDisposable
    {

        /// <summary>
        /// The memory Stream that is used to simulate
        /// a file that is read by the dxf parsers
        /// </summary>
        protected MemoryStream _memoryStream;

        /// <summary>
        /// Takes the data string, writes to the memory buffer,
        /// reads the dxffile and extracts the first entity that
        /// matches the type given.
        /// </summary>
        /// <param name="data">The data that represents a dxffile</param>
        /// <typeparam name="T">The entity type</typeparam>
        /// <returns>Returns the First Entity from the Dxf File that matches the type given</returns>
        protected T GetFirstEntity<T>(string data)
        {
            WriteMemory(data);
            var dxfFile = new DxfFile(_memoryStream);
            var entity = dxfFile.GetEntities<T>();
            return entity.FirstOrDefault();
        }

        /// <summary>
        /// Takes the data string, writes to the memory buffer,
        /// reads the dxf file and extracts all of the entities
        /// </summary>
        /// <param name="data">The data that represents a dxf file</param>
        /// <returns>Returns: A list of all the entities from the file</returns>
        protected List<IEntity> GetAllEntities(string data)
        {
            WriteMemory(data);
            var dxfFile = new DxfFile(_memoryStream);
            return dxfFile.Entities;
        }

        /// <summary>
        /// A Stream Writer that is used to write to the memory stream.
        /// Note that this is not disposed of until the object is distroyed to keep
        /// the base stream alive.
        /// </summary>
        private StreamWriter _streamWriter;

        /// <summary>
        /// Default Constructor for the EntityBaseClass.
        /// This Constructor initalizes the streams and provides a
        /// logger from the TestBase Class
        /// </summary>
        /// <param name="logger">Logger passed from XUnit</param>
        public EntityTestBase(ITestOutputHelper logger) : base(logger)
        {
            // Initialize the streams
            _memoryStream = new MemoryStream();
            _streamWriter = new StreamWriter(_memoryStream);
        }

        /// <summary>
        /// Dispose of the streams and other managed resources
        /// </summary>
        public void Dispose()
        {
            _streamWriter.Dispose();
            _memoryStream.Dispose();
        }

        /// <summary>
        /// Write to the memory stream.
        /// Note that after writing to the stream the position
        /// of the memory stream is reset to 0
        /// </summary>
        /// <param name="value">The string that is written to the stream</param>
        protected void WriteMemory(string value)
        {
            _streamWriter.Write(value);

            // Make sure to flush and reset the position of the stream
            _streamWriter.Flush();
            _memoryStream.Position = 0;
        }
    }
}