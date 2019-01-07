// EntityParser.cs
// Created by: Adam Renaud
// Created on: 2019-01-06

using System;
using System.Linq;
using System.Collections.Generic;

using DxfLibrary.Parse;
using DxfLibrary.Entities;
using DxfLibrary.IO;
using DxfLibrary.DxfSpec;

namespace DxfLibrary.Parse.Entities
{
    /// <summary>
    /// A base parsing class for the entity type
    /// </summary>
    public class EntityParser<T> : IDxfEntityParser<T, string, object, object>
    {
        public bool BaseParse(IEntity entity, TaggedData<string, object> data, IDxfSpec<object> entitySpec)
        {
            var properties = entity.GetType().GetProperties();

            // Find properties that match the current data and set them 
            var property = properties
                .Where(prop => entitySpec.Properties
                .Any(s => s.Name as string == prop.Name 
                && data.GroupCode == s.Code as string))
                .FirstOrDefault();

            // If the property is not null then try to set it
            // if unable to set it return false, otherwise return true
            if (property != null)
            {
                try
                {
                    entity.SetProperty(property.Name, data.Value);
                }
                catch(Exception)
                {
                    throw;
                }
                return true;
            }

            // If unable to set a property then return
            return false;
        }

        public T ParseEntity(IEntity entity, IDxfReader<string, object> reader, IDxfSpec<object> entitySpec)
        {
            var properties = typeof(T).GetProperties();

            var commonSpec = SpecService.GetSpec<object>(SpecService.DxfCommonSpec);

            var baseEntitySpec = SpecService.GetSpec<object>(SpecService.EntitySpec);

            while(!reader.EndOfStream)
            {
                var data = reader.GetNextPair();
                
                if (commonSpec.Get("Sections.EndCode") as string == data.GroupCode )
                    break;

                if (BaseParse(entity, data, baseEntitySpec))
                    continue;

                var query = properties
                    .Where(prop => entitySpec.Properties
                    .Any(spec => spec.Name == prop.Name && spec.Code as string == data.GroupCode))
                    .FirstOrDefault();

                if (query != null)
                    entity.SetProperty(query.Name, data.Value);
            }

            return (T)entity;
        }
    }
}