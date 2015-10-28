﻿using EntityJoke.Structure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EntityJoke.Core
{
    public class EntityLoader
    {
        internal DataRow Row;
        internal int IndexColumn;
        internal Entity Entity;
        internal DataColumnCollection Columns;
        private object obj;

        internal object LoadInstance()
        {
            obj = Activator.CreateInstance(Entity.Type);
            ProccesColumns();
            return obj;
        }

        private void ProccesColumns()
        {
            int limiteLoop = EntityColumnsLength();

            for (; IndexColumn < limiteLoop; IndexColumn++)
                ProcessField();

            Entity.Joins.ForEach(j => ProcessJoin(j));
        }

        private int EntityColumnsLength()
        {
            int fieldsNotEntity = Entity.GetFields().Where(f => !f.IsEntity).ToList().Count;
            return fieldsNotEntity + IndexColumn;
        }

        private void ProcessField()
        {
            DataColumn column = GetCurrentColumn();
            var value = Row[column.ColumnName];

            if (!IsNullValue(value))
                SetFieldValue(GetColumnField(column), value);
        }

        private DataColumn GetCurrentColumn()
        {
            return Columns[IndexColumn];
        }

        private bool IsNullValue(object value)
        {
            return value.GetType() == typeof(DBNull);
        }

        private Field GetColumnField(DataColumn column)
        {
            return Entity.FieldDictionary[GetOriginalName(column)];
        }

        private string GetOriginalName(DataColumn column)
        {
            int indexOf = column.ColumnName.IndexOf("_");
            return column.ColumnName.Substring(indexOf + 1);
        }

        private void SetFieldValue(Field field, object value)
        {
            new FieldValueSetter(obj, field, value).Set();
        }

        private void ProcessJoin(EntityJoin join)
        {
            SetFieldValue(join.Field, GetJoinValue(join));
        }

        private object GetJoinValue(EntityJoin join)
        {
            return new EntityLoaderBuilder()
                .Entity(join.Entity)
                .Row(Row)
                .Columns(Columns)
                .IndexColumn(IndexColumn)
                .Build();
        }
    }
}
