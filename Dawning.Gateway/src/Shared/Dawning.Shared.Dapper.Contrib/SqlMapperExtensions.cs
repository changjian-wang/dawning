using System.Data;
using System.Reflection;
using System.Text;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Linq;
using Dapper;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Shared.Dapper.Contrib
{
    /// <summary>
    /// The Dapper.Contrib extensions for Dapper
    /// </summary>
    public static partial class SqlMapperExtensions
    {
        /// <summary>
        /// Defined a proxy object with a possibly dirty state.
        /// </summary>
        public interface IProxy //must be kept public
        {
            /// <summary>
            /// Whether the object has been changed.
            /// </summary>
            bool IsDirty { get; set; }
        }

        /// <summary>
        /// Defines a table name mapper for getting table names from types.
        /// </summary>
        public interface ITableNameMapper
        {
            /// <summary>
            /// Gets a table name from a given <see cref="Type"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> to get a name from.</param>
            /// <returns>The table name for the given <paramref name="type"/>.</returns>
            string GetTableName(Type type);
        }

        /// <summary>
        /// The function to get a database type from the given <see cref="IDbConnection"/>.
        /// </summary>
        /// <param name="connection">The connection to get a database type name from.</param>
        public delegate string GetDatabaseTypeDelegate(IDbConnection connection);
        /// <summary>
        /// The function to get a table name from a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to get a table name for.</param>
        public delegate string TableNameMapperDelegate(Type type);

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ExplicitKeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> IgnoreUpdateProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableName = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        private static readonly ISqlAdapter DefaultAdapter = new SqlServerAdapter();
        private static readonly Dictionary<string, ISqlAdapter> AdapterDictionary
            = new Dictionary<string, ISqlAdapter>(6)
            {
                ["sqlconnection"] = new SqlServerAdapter(),
                ["sqlceconnection"] = new SqlCeServerAdapter(),
                ["npgsqlconnection"] = new PostgresAdapter(),
                ["sqliteconnection"] = new SQLiteAdapter(),
                ["mysqlconnection"] = new MySqlAdapter(),
                ["fbconnection"] = new FbAdapter()
            };

        private static List<PropertyInfo> ComputedPropertiesCache(Type type)
        {
            if (ComputedProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            var computedProperties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ComputedAttribute)).ToList();

            ComputedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }

        private static List<PropertyInfo> IgnoreUpdatePropertiesCache(Type type)
        {
            if (IgnoreUpdateProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            var ignoreUpdateProperties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is IgnoreUpdateAttribute)).ToList();

            IgnoreUpdateProperties[type.TypeHandle] = ignoreUpdateProperties;
            return ignoreUpdateProperties;
        }

        private static List<PropertyInfo> ExplicitKeyPropertiesCache(Type type)
        {
            if (ExplicitKeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            var explicitKeyProperties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute)).ToList();

            ExplicitKeyProperties[type.TypeHandle] = explicitKeyProperties;
            return explicitKeyProperties;
        }

        private static List<PropertyInfo> KeyPropertiesCache(Type type)
        {
            if (KeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            var allProperties = TypePropertiesCache(type);
            var keyProperties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).ToList();

            if (keyProperties.Count == 0)
            {
                var idProp = allProperties.Find(p => string.Equals(p.Name, "id", StringComparison.CurrentCultureIgnoreCase));
                if (idProp != null && !idProp.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute))
                {
                    keyProperties.Add(idProp);
                }
            }

            KeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        private static List<PropertyInfo> TypePropertiesCache(Type type)
        {
            if (TypeProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pis))
            {
                return pis.ToList();
            }

            var properties = type.GetProperties().Where(IsWriteable).ToArray();
            TypeProperties[type.TypeHandle] = properties;
            return properties.ToList();
        }

        private static bool IsWriteable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(WriteAttribute), false).AsList();
            if (attributes.Count != 1) return true;

            var writeAttribute = (WriteAttribute)attributes[0];
            return writeAttribute.Write;
        }

        private static PropertyInfo GetSingleKey<T>(string method)
        {
            var type = typeof(T);
            var keys = KeyPropertiesCache(type);
            var explicitKeys = ExplicitKeyPropertiesCache(type);
            var keyCount = keys.Count + explicitKeys.Count;
            if (keyCount > 1)
                throw new DataException($"{method}<T> only supports an entity with a single [Key] or [ExplicitKey] property. [Key] Count: {keys.Count}, [ExplicitKey] Count: {explicitKeys.Count}");
            if (keyCount == 0)
                throw new DataException($"{method}<T> only supports an entity with a [Key] or an [ExplicitKey] property");

            return keys.Count > 0 ? keys[0] : explicitKeys[0];
        }

        /// <summary>
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
        {
            var type = typeof(T);

            if (!GetQueries.TryGetValue(type.TypeHandle, out string? sql))
            {
                var property = GetSingleKey<T>(nameof(Get));
                var key = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;
                var name = GetTableName(type);

                sql = $"select * from {name} where {key} = @id";
                GetQueries[type.TypeHandle] = sql;
            }

            var dynParams = new DynamicParameters();
            dynParams.Add("@id", id);

            var obj = connection.Query(sql, dynParams, transaction, commandTimeout: commandTimeout).FirstOrDefault();
            return GetImpl<T>(obj, type);
        }

        /// <summary>
        /// Returns an entity from table "Ts".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static T GetImpl<T>(dynamic data, Type type) where T : class, new()
        {
            T obj = new T();

            foreach (var property in TypePropertiesCache(type))
            {
                var name = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;
                var res = data as IDictionary<string, object>;
                var val = res[name];
                if (val == null) continue;
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var genericType = Nullable.GetUnderlyingType(property.PropertyType);
                    if (genericType != null) property.SetValue(obj, Convert.ChangeType(val, genericType), null);
                }
                else
                {
                    property.SetValue(obj, Convert.ChangeType(val, property.PropertyType), null);
                }
            }

            return obj;
        }

        private static IEnumerable<T> GetListImpl<T>(IEnumerable<dynamic> data, Type type) where T : class, new()
        {
            var list = new List<T>();

            foreach (IDictionary<string, object> res in data)
            {
                T obj = new T();
                foreach (var property in TypePropertiesCache(type))
                {
                    var name = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;

                    // ✅ 使用 TryGetValue
                    if (!res.TryGetValue(name, out var val))
                    {
                        continue;
                    }

                    // ✅ 处理 DBNull 和 null
                    if (val == null || val is DBNull)
                    {
                        if (property.PropertyType.IsGenericType &&
                            property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            property.SetValue(obj, null, null);
                        }
                        continue;
                    }

                    try
                    {
                        // ✅ 安全的类型转换
                        if (property.PropertyType.IsGenericType &&
                            property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                            property.SetValue(obj, Convert.ChangeType(val, underlyingType!), null);
                        }
                        else if (property.PropertyType == typeof(Guid))
                        {
                            property.SetValue(obj, val is Guid guid ? guid : Guid.Parse(val.ToString()!), null);
                        }
                        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        {
                            // 直接设置DateTime，避免Convert.ChangeType的类型转换问题
                            property.SetValue(obj, val is DateTime dt ? dt : DateTime.Parse(val.ToString()!), null);
                        }
                        else if (property.PropertyType.IsEnum)
                        {
                            property.SetValue(obj, Enum.ToObject(property.PropertyType, val), null);
                        }
                        else
                        {
                            property.SetValue(obj, Convert.ChangeType(val, property.PropertyType), null);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to set property '{property.Name}' of type '{property.PropertyType}' with value '{val}' of type '{val?.GetType()}'",
                            ex);
                    }
                }
                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// Returns a list of entities from table "Ts".
        /// Id of T must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance.
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public static IEnumerable<T> GetAll<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
        {
            var type = typeof(T);
            var cacheType = typeof(List<T>);

            if (!GetQueries.TryGetValue(cacheType.TypeHandle, out string? sql))
            {
                GetSingleKey<T>(nameof(GetAll));
                var name = GetTableName(type);

                sql = "select * from " + name;
                GetQueries[cacheType.TypeHandle] = sql;
            }

            var result = connection.Query(sql);
            return GetListImpl<T>(result, type);
        }

        /// <summary>
        /// Specify a custom table name mapper based on the POCO type name
        /// </summary>
#pragma warning disable CA2211 // Non-constant fields should not be visible - I agree with you, but we can't do that until we break the API
        public static TableNameMapperDelegate TableNameMapper;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        private static string GetTableName(Type type)
        {
            if (TypeTableName.TryGetValue(type.TypeHandle, out string name)) return name;

            if (TableNameMapper != null)
            {
                name = TableNameMapper(type);
            }
            else
            {
                //NOTE: This as dynamic trick falls back to handle both our own Table-attribute as well as the one in EntityFramework 
                var tableAttrName =
                    type.GetCustomAttribute<TableAttribute>(false)?.Name
                    ?? (type.GetCustomAttributes(false).FirstOrDefault(attr => attr.GetType().Name == "TableAttribute") as dynamic)?.Name;

                if (tableAttrName != null)
                {
                    name = tableAttrName;
                }
                else
                {
                    name = type.Name + "s";
                    if (type.IsInterface && name.StartsWith("I"))
                        name = name.Substring(1);
                }
            }

            TypeTableName[type.TypeHandle] = name;
            return name;
        }

        /// <summary>
        /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <typeparam name="T">The type to insert.</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToInsert">Entity to insert, can be list of entities</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Identity of inserted entity, or number of inserted rows if inserting a list</returns>
        public static long Insert<T>(this IDbConnection connection, T entityToInsert, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
        {
            var isList = false;

            var type = typeof(T);

            if (type.IsArray)
            {
                isList = true;
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                var typeInfo = type.GetTypeInfo();
                bool implementsGenericIEnumerableOrIsGenericIEnumerable =
                    typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                    typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsGenericIEnumerableOrIsGenericIEnumerable)
                {
                    isList = true;
                    type = type.GetGenericArguments()[0];
                }
            }

            var name = GetTableName(type);
            var sbColumnList = new StringBuilder(null);
            var allProperties = TypePropertiesCache(type);
            var keyProperties = KeyPropertiesCache(type);
            var computedProperties = ComputedPropertiesCache(type);
            var allPropertiesExceptKeyAndComputed = allProperties.Except(keyProperties.Union(computedProperties)).ToList();

            var adapter = GetFormatter(connection);

            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count; i++)
            {
                var property = allPropertiesExceptKeyAndComputed[i];
                string columnName = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;
                adapter.AppendColumnName(sbColumnList, columnName);  //fix for issue #336
                if (i < allPropertiesExceptKeyAndComputed.Count - 1)
                    sbColumnList.Append(", ");
            }

            var sbParameterList = new StringBuilder(null);
            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count; i++)
            {
                var property = allPropertiesExceptKeyAndComputed[i];
                sbParameterList.AppendFormat("@{0}", property.Name);
                if (i < allPropertiesExceptKeyAndComputed.Count - 1)
                    sbParameterList.Append(", ");
            }

            int returnVal;
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed) connection.Open();

            if (!isList)    //single entity
            {
                returnVal = adapter.Insert(connection, transaction, commandTimeout, name, sbColumnList.ToString(),
                    sbParameterList.ToString(), keyProperties, entityToInsert);
            }
            else
            {
                //insert list of entities
                var cmd = $"insert into {name} ({sbColumnList}) values ({sbParameterList})";
                returnVal = connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
            }
            if (wasClosed) connection.Close();
            return returnVal;
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToUpdate">Entity to be updated</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static bool Update<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
        {
            if (entityToUpdate is IProxy proxy && !proxy.IsDirty)
            {
                return false;
            }

            var type = typeof(T);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                var typeInfo = type.GetTypeInfo();
                bool implementsGenericIEnumerableOrIsGenericIEnumerable =
                    typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                    typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsGenericIEnumerableOrIsGenericIEnumerable)
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            var keyProperties = KeyPropertiesCache(type).ToList();  //added ToList() due to issue #418, must work on a list copy
            var explicitKeyProperties = ExplicitKeyPropertiesCache(type);
            if (keyProperties.Count == 0 && explicitKeyProperties.Count == 0)
                throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");

            var name = GetTableName(type);

            var sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", name);

            var allProperties = TypePropertiesCache(type);
            keyProperties.AddRange(explicitKeyProperties);
            var computedProperties = ComputedPropertiesCache(type);
            var ignoreUpdateProperties = IgnoreUpdatePropertiesCache(type);
            var nonIdProps = allProperties.Except(keyProperties.Union(computedProperties).Union(ignoreUpdateProperties)).ToList();

            var adapter = GetFormatter(connection);

            for (var i = 0; i < nonIdProps.Count; i++)
            {
                var property = nonIdProps[i];
                adapter.AppendColumnNameEqualsValue(sb, property);  //fix for issue #336
                if (i < nonIdProps.Count - 1)
                    sb.Append(", ");
            }
            sb.Append(" where ");
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties[i];
                adapter.AppendColumnNameEqualsValue(sb, property);  //fix for issue #336
                if (i < keyProperties.Count - 1)
                    sb.Append(" and ");
            }
            var updated = connection.Execute(sb.ToString(), entityToUpdate, commandTimeout: commandTimeout, transaction: transaction);
            return updated > 0;
        }

        /// <summary>
        /// Delete entity in table "Ts".
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToDelete">Entity to delete</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if not found</returns>
        public static bool Delete<T>(this IDbConnection connection, T entityToDelete, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
        {
            if (entityToDelete == null)
                throw new ArgumentException("Cannot Delete null Object", nameof(entityToDelete));

            var type = typeof(T);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                var typeInfo = type.GetTypeInfo();
                bool implementsGenericIEnumerableOrIsGenericIEnumerable =
                    typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                    typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsGenericIEnumerableOrIsGenericIEnumerable)
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            var keyProperties = KeyPropertiesCache(type).ToList();  //added ToList() due to issue #418, must work on a list copy
            var explicitKeyProperties = ExplicitKeyPropertiesCache(type);
            if (keyProperties.Count == 0 && explicitKeyProperties.Count == 0)
                throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");

            var name = GetTableName(type);
            keyProperties.AddRange(explicitKeyProperties);

            var sb = new StringBuilder();
            sb.AppendFormat("delete from {0} where ", name);

            var adapter = GetFormatter(connection);

            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties[i];
                adapter.AppendColumnNameEqualsValue(sb, property);  //fix for issue #336
                if (i < keyProperties.Count - 1)
                    sb.Append(" and ");
            }
            var deleted = connection.Execute(sb.ToString(), entityToDelete, transaction, commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        /// Delete all entities in the table related to the type T.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if none found</returns>
        public static bool DeleteAll<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            var name = GetTableName(type);
            var statement = $"delete from {name}";
            var deleted = connection.Execute(statement, null, transaction, commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        /// Specifies a custom callback that detects the database type instead of relying on the default strategy (the name of the connection type object).
        /// Please note that this callback is global and will be used by all the calls that require a database specific adapter.
        /// </summary>
#pragma warning disable CA2211 // Non-constant fields should not be visible - I agree with you, but we can't do that until we break the API
        public static GetDatabaseTypeDelegate GetDatabaseType;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        private static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            var name = GetDatabaseType?.Invoke(connection).ToLower()
                       ?? connection.GetType().Name.ToLower();

            return AdapterDictionary.TryGetValue(name, out var adapter)
                ? adapter
                : DefaultAdapter;
        }

        private static class ProxyGenerator
        {
            private static readonly Dictionary<Type, Type> TypeCache = new Dictionary<Type, Type>();

            private static AssemblyBuilder GetAsmBuilder(string name)
            {
#if !NET461
                return AssemblyBuilder.DefineDynamicAssembly(new AssemblyName { Name = name }, AssemblyBuilderAccess.Run);
#else
                return Thread.GetDomain().DefineDynamicAssembly(new AssemblyName { Name = name }, AssemblyBuilderAccess.Run);
#endif
            }

            public static T GetInterfaceProxy<T>()
            {
                Type typeOfT = typeof(T);

                if (TypeCache.TryGetValue(typeOfT, out Type k))
                {
                    return (T)Activator.CreateInstance(k);
                }
                var assemblyBuilder = GetAsmBuilder(typeOfT.Name);

                var moduleBuilder = assemblyBuilder.DefineDynamicModule("SqlMapperExtensions." + typeOfT.Name); //NOTE: to save, add "asdasd.dll" parameter

                var interfaceType = typeof(IProxy);
                var typeBuilder = moduleBuilder.DefineType(typeOfT.Name + "_" + Guid.NewGuid(),
                    TypeAttributes.Public | TypeAttributes.Class);
                typeBuilder.AddInterfaceImplementation(typeOfT);
                typeBuilder.AddInterfaceImplementation(interfaceType);

                //create our _isDirty field, which implements IProxy
                var setIsDirtyMethod = CreateIsDirtyProperty(typeBuilder);

                // Generate a field for each property, which implements the T
                foreach (var property in typeof(T).GetProperties())
                {
                    var isId = property.GetCustomAttributes(true).Any(a => a is KeyAttribute);
                    CreateProperty<T>(typeBuilder, property.Name, property.PropertyType, setIsDirtyMethod, isId);
                }

#if NETSTANDARD2_0
                var generatedType = typeBuilder.CreateTypeInfo().AsType();
#else
                var generatedType = typeBuilder.CreateType();
#endif

                TypeCache.Add(typeOfT, generatedType);
                return (T)Activator.CreateInstance(generatedType);
            }

            private static MethodInfo CreateIsDirtyProperty(TypeBuilder typeBuilder)
            {
                var propType = typeof(bool);
                var field = typeBuilder.DefineField("_" + nameof(IProxy.IsDirty), propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty(nameof(IProxy.IsDirty),
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new[] { propType });

                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.SpecialName
                                                  | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + nameof(IProxy.IsDirty),
                                             getSetAttr,
                                             propType,
                                             Type.EmptyTypes);
                var currGetIl = currGetPropMthdBldr.GetILGenerator();
                currGetIl.Emit(OpCodes.Ldarg_0);
                currGetIl.Emit(OpCodes.Ldfld, field);
                currGetIl.Emit(OpCodes.Ret);
                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + nameof(IProxy.IsDirty),
                                             getSetAttr,
                                             null,
                                             new[] { propType });
                var currSetIl = currSetPropMthdBldr.GetILGenerator();
                currSetIl.Emit(OpCodes.Ldarg_0);
                currSetIl.Emit(OpCodes.Ldarg_1);
                currSetIl.Emit(OpCodes.Stfld, field);
                currSetIl.Emit(OpCodes.Ret);

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(IProxy).GetMethod("get_" + nameof(IProxy.IsDirty));
                var setMethod = typeof(IProxy).GetMethod("set_" + nameof(IProxy.IsDirty));
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);

                return currSetPropMthdBldr;
            }

            private static void CreateProperty<T>(TypeBuilder typeBuilder, string propertyName, Type propType, MethodInfo setIsDirtyMethod, bool isIdentity)
            {
                //Define the field and the property 
                var field = typeBuilder.DefineField("_" + propertyName, propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty(propertyName,
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new[] { propType });

                const MethodAttributes getSetAttr = MethodAttributes.Public
                                                    | MethodAttributes.Virtual
                                                    | MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName,
                                             getSetAttr,
                                             propType,
                                             Type.EmptyTypes);

                var currGetIl = currGetPropMthdBldr.GetILGenerator();
                currGetIl.Emit(OpCodes.Ldarg_0);
                currGetIl.Emit(OpCodes.Ldfld, field);
                currGetIl.Emit(OpCodes.Ret);

                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                                             getSetAttr,
                                             null,
                                             new[] { propType });

                //store value in private field and set the isdirty flag
                var currSetIl = currSetPropMthdBldr.GetILGenerator();
                currSetIl.Emit(OpCodes.Ldarg_0);
                currSetIl.Emit(OpCodes.Ldarg_1);
                currSetIl.Emit(OpCodes.Stfld, field);
                currSetIl.Emit(OpCodes.Ldarg_0);
                currSetIl.Emit(OpCodes.Ldc_I4_1);
                currSetIl.Emit(OpCodes.Call, setIsDirtyMethod);
                currSetIl.Emit(OpCodes.Ret);

                //TODO: Should copy all attributes defined by the interface?
                if (isIdentity)
                {
                    var keyAttribute = typeof(KeyAttribute);
                    var myConstructorInfo = keyAttribute.GetConstructor(Type.EmptyTypes);
                    var attributeBuilder = new CustomAttributeBuilder(myConstructorInfo, Array.Empty<object>());
                    property.SetCustomAttribute(attributeBuilder);
                }

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(T).GetMethod("get_" + propertyName);
                var setMethod = typeof(T).GetMethod("set_" + propertyName);
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);
            }
        }


        #region Build Conditions

        /// <summary>
        /// 创建查询构建器
        /// </summary>
        public static QueryBuilder<TEntity> Builder<TEntity>(
            this IDbConnection connection,
            IDbTransaction? transaction = null,
            int? commandTimeout = null)
            where TEntity : class, new()
        {
            return new QueryBuilder<TEntity>(connection, transaction, commandTimeout);
        }

        public partial class QueryBuilder<TEntity> where TEntity : class, new()
        {
            private readonly IDbConnection _connection;
            private readonly IDbTransaction? _transaction;
            private readonly int? _commandTimeout;
            private readonly List<string> _conditions = new List<string>();
            private string _orderBy = "";
            private bool _orderByDescending = true;
            private ISqlAdapter sqlAdapter;
            private readonly Dictionary<string, object?> _parameters = new Dictionary<string, object?>();

            public QueryBuilder(IDbConnection connection, IDbTransaction? transaction, int? commandTimeout)
            {
                sqlAdapter ??= GetFormatter(connection);
                _connection = connection;
                _transaction = transaction;
                _commandTimeout = commandTimeout;
                SetDefaultOrder();
            }

            public QueryBuilder<TEntity> WhereIf(bool exists, Expression<Func<TEntity, bool>> predicate)
            {
                if (exists)
                {
                    Visit(predicate, _conditions);
                }

                return this;
            }

            public QueryBuilder<TEntity> OrderBy<T>(Expression<Func<TEntity, T>> expression)
            {
                _orderBy = GetMemberName(expression);
                _orderByDescending = false;
                return this;
            }

            public QueryBuilder<TEntity> OrderByDescending<T>(Expression<Func<TEntity, T>> expression)
            {
                _orderBy = GetMemberName(expression);
                _orderByDescending = true;
                return this;
            }

            public PagedResult<TEntity> AsPagedList(int page, int itemsPerPage)
            {
                if (page < 1) page = 1;
                if (itemsPerPage < 1) itemsPerPage = 1;

                var type = typeof(TEntity);
                var name = GetTableName(type);

                // 构建 WHERE 子句（使用空格连接，保持 AND/OR/括号）
                string whereClause = _conditions.Count > 0 ? string.Join(" ", _conditions) : "1=1";

                // 转换参数
                var parameters = ConvertToDynamicParameters();

                // 确保有排序列
                if (string.IsNullOrEmpty(_orderBy))
                {
                    throw new InvalidOperationException(
                        "A sorting column must be specified for pagination. " +
                        "Use OrderBy() or OrderByDescending(), or add [DefaultSortName] attribute to a property.");
                }

                // 获取分页数据
                var list = sqlAdapter.RetrieveCurrentPaginatedData(
                    _connection,
                    _transaction,
                    _commandTimeout,
                    name,
                    _orderBy,
                    page,
                    itemsPerPage,
                    whereClause,
                    parameters);

                // 获取总数
                var countSql = $"SELECT COUNT(*) FROM {name} WHERE {whereClause}";
                var count = _connection.ExecuteScalar(countSql, parameters, _transaction, _commandTimeout);
                long totalCount = count != null ? Convert.ToInt64(count) : 0;

                return new PagedResult<TEntity>
                {
                    Values = GetListImpl<TEntity>(list, type),
                    ItemsPerPage = itemsPerPage,
                    Page = page,
                    TotalItems = totalCount
                };
            }

            public IEnumerable<TEntity> AsList()
            {
                var type = typeof(TEntity);
                var name = GetTableName(type);

                // 构建 WHERE 子句
                string whereClause = _conditions.Count > 0 ? string.Join(" ", _conditions) : "1=1";

                // 转换参数
                var parameters = ConvertToDynamicParameters();

                // 构建 SQL
                var sql = $"SELECT * FROM {name} WHERE {whereClause}";

                // 添加排序
                if (!string.IsNullOrEmpty(_orderBy))
                {
                    sql += $" ORDER BY {sqlAdapter.ConvertColumnName(_orderBy)} {(_orderByDescending ? "DESC" : "ASC")}";
                }

                var list = _connection.Query(sql, parameters, _transaction, commandTimeout: _commandTimeout);
                IEnumerable<TEntity> entities = GetListImpl<TEntity>(list, type);

                return entities;
            }

            private void Visit(Expression expression, List<string> conditions)
            {
                string memberName;
                object? value;
                string paramName;

                switch (expression.NodeType)
                {
                    case ExpressionType.Lambda:
                        Visit(((LambdaExpression)expression).Body, conditions);
                        break;

                    case ExpressionType.AndAlso:
                        var binaryAnd = (BinaryExpression)expression;
                        Visit(binaryAnd.Left, conditions);
                        conditions.Add("AND");
                        Visit(binaryAnd.Right, conditions);
                        break;

                    case ExpressionType.OrElse:
                        var binaryOr = (BinaryExpression)expression;
                        conditions.Add("(");
                        Visit(binaryOr.Left, conditions);
                        conditions.Add("OR");
                        Visit(binaryOr.Right, conditions);
                        conditions.Add(")");
                        break;

                    case ExpressionType.Equal:
                        var binaryEqual = (BinaryExpression)expression;
                        memberName = GetMemberName(binaryEqual.Left);
                        value = GetValueFromExpression(binaryEqual.Right);

                        // ✅ 处理 null 比较
                        if (value == null)
                        {
                            conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} IS NULL");
                            break;
                        }

                        paramName = GetUniqueParameterName(memberName);
                        _parameters[paramName] = value;
                        conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} = {paramName}");
                        break;

                    case ExpressionType.NotEqual:
                        var binaryNotEqual = (BinaryExpression)expression;
                        memberName = GetMemberName(binaryNotEqual.Left);
                        value = GetValueFromExpression(binaryNotEqual.Right);

                        // ✅ 处理 null 比较
                        if (value == null)
                        {
                            conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} IS NOT NULL");
                            break;
                        }

                        paramName = GetUniqueParameterName(memberName);
                        _parameters[paramName] = value;
                        conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} <> {paramName}");
                        break;

                    case ExpressionType.GreaterThan:
                        var binaryGreaterThan = (BinaryExpression)expression;
                        memberName = GetMemberName(binaryGreaterThan.Left);
                        value = GetValueFromExpression(binaryGreaterThan.Right);
                        paramName = GetUniqueParameterName(memberName);
                        _parameters[paramName] = value;
                        conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} > {paramName}");
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        var binaryGreaterThanOrEqual = (BinaryExpression)expression;
                        memberName = GetMemberName(binaryGreaterThanOrEqual.Left);
                        value = GetValueFromExpression(binaryGreaterThanOrEqual.Right);
                        paramName = GetUniqueParameterName(memberName);
                        _parameters[paramName] = value;
                        conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} >= {paramName}");
                        break;

                    case ExpressionType.LessThan:
                        var binaryLessThan = (BinaryExpression)expression;
                        memberName = GetMemberName(binaryLessThan.Left);
                        value = GetValueFromExpression(binaryLessThan.Right);
                        paramName = GetUniqueParameterName(memberName);
                        _parameters[paramName] = value;
                        conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} < {paramName}");
                        break;

                    case ExpressionType.LessThanOrEqual:
                        var binaryLessThanOrEqual = (BinaryExpression)expression;
                        memberName = GetMemberName(binaryLessThanOrEqual.Left);
                        value = GetValueFromExpression(binaryLessThanOrEqual.Right);
                        paramName = GetUniqueParameterName(memberName);
                        _parameters[paramName] = value;
                        conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} <= {paramName}");
                        break;

                    case ExpressionType.Call:
                        var methodCall = (MethodCallExpression)expression;
                        if (methodCall.Method.Name == "StartsWith")
                        {
                            memberName = GetMemberName(methodCall.Object);
                            value = GetValueFromExpression(methodCall.Arguments[0]);

                            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                                break;

                            paramName = GetUniqueParameterName(memberName);
                            var escapedValue = EscapeLikeValue(value.ToString()!, sqlAdapter);
                            _parameters[paramName] = $"{escapedValue}%";

                            var likeCondition = BuildLikeCondition(memberName, paramName, sqlAdapter);
                            conditions.Add(likeCondition);
                        }
                        else if (methodCall.Method.Name == "EndsWith")
                        {
                            memberName = GetMemberName(methodCall.Object);
                            value = GetValueFromExpression(methodCall.Arguments[0]);

                            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                                break;

                            paramName = GetUniqueParameterName(memberName);
                            var escapedValue = EscapeLikeValue(value.ToString()!, sqlAdapter);
                            _parameters[paramName] = $"%{escapedValue}";

                            var likeCondition = BuildLikeCondition(memberName, paramName, sqlAdapter);
                            conditions.Add(likeCondition);
                        }
                        else if (methodCall.Method.Name == "Equals")
                        {
                            memberName = GetMemberName(methodCall.Object);
                            value = GetValueFromExpression(methodCall.Arguments[0]);

                            // ✅ 处理 null 值
                            if (value == null)
                            {
                                conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} IS NULL");
                                break;
                            }

                            paramName = GetUniqueParameterName(memberName);
                            _parameters[paramName] = value;
                            conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} = {paramName}");
                        }
                        else if (methodCall.Method.Name == "Contains")
                        {
                            if (methodCall.Method.DeclaringType == typeof(string))
                            {
                                // ✅ 字符串的 Contains: x.Name.Contains("test")
                                memberName = GetMemberName(methodCall.Object);
                                value = GetValueFromExpression(methodCall.Arguments[0]);

                                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                                    break;

                                paramName = GetUniqueParameterName(memberName);
                                var escapedValue = EscapeLikeValue(value.ToString()!, sqlAdapter);
                                _parameters[paramName] = $"%{escapedValue}%";

                                var likeCondition = BuildLikeCondition(memberName, paramName, sqlAdapter);
                                conditions.Add(likeCondition);
                            }
                            else
                            {
                                // ✅ 集合的 Contains: list.Contains(x.Name)
                                memberName = GetMemberName(methodCall.Arguments[0]);
                                value = GetValueFromExpression(methodCall.Object);

                                if (value == null)
                                {
                                    conditions.Add("1 = 0");
                                    break;
                                }

                                // ✅ 检查集合是否为空
                                if (value is System.Collections.IEnumerable enumerable)
                                {
                                    var enumerator = enumerable.GetEnumerator();
                                    if (!enumerator.MoveNext())
                                    {
                                        conditions.Add("1 = 0");
                                        break;
                                    }
                                }

                                paramName = GetUniqueParameterName(memberName);
                                _parameters[paramName] = value;
                                conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} IN {paramName}");
                            }
                        }
                        break;

                    case ExpressionType.Not:
                        if (expression is UnaryExpression unary && unary.Operand is MethodCallExpression notContains)
                        {
                            if (notContains.Method.Name == "Contains")
                            {
                                memberName = GetMemberName(notContains.Arguments[0]);
                                value = GetValueFromExpression(notContains.Object);

                                if (value == null)
                                {
                                    conditions.Add("1 = 1");
                                    break;
                                }

                                paramName = GetUniqueParameterName(memberName);
                                _parameters[paramName] = value;
                                conditions.Add($"{sqlAdapter.ConvertColumnName(memberName)} NOT IN {paramName}");
                            }
                        }
                        break;

                    case ExpressionType.Convert:
                    case ExpressionType.TypeAs:
                        Visit(((UnaryExpression)expression).Operand, conditions);
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported expression type: {expression.NodeType}");
                }
            }

            private string EscapeLikeValue(string value, ISqlAdapter adapter)
            {
                if (string.IsNullOrEmpty(value))
                    return value;

                if (adapter is MySqlAdapter || adapter is PostgresAdapter || adapter is SQLiteAdapter)
                {
                    // MySQL, PostgreSQL, SQLite 使用反斜杠转义
                    return value
                        .Replace("\\", "\\\\")
                        .Replace("%", "\\%")
                        .Replace("_", "\\_");
                }
                else
                {
                    // SQL Server 使用方括号转义
                    return value
                        .Replace("[", "[[]")
                        .Replace("%", "[%]")
                        .Replace("_", "[_]");
                }
            }

            /// <summary>
            /// 构建 LIKE 条件（某些数据库需要 ESCAPE 子句）
            /// </summary>
            private static string BuildLikeCondition(string memberName, string paramName, ISqlAdapter adapter)
            {
                var columnName = adapter.ConvertColumnName(memberName);

                // SQLite 和 Firebird 需要显式指定 ESCAPE 子句
                if (adapter is SQLiteAdapter || adapter is FbAdapter)
                {
                    return $"{columnName} LIKE {paramName} ESCAPE '\\'";
                }

                // PostgreSQL 在某些情况下也需要 ESCAPE
                if (adapter is PostgresAdapter)
                {
                    return $"{columnName} LIKE {paramName} ESCAPE '\\'";
                }

                // SQL Server, MySQL 默认支持转义
                return $"{columnName} LIKE {paramName}";
            }

            private string GetUniqueParameterName(string columnName)
            {
                var baseParamName = $"@{columnName}";

                if (!_parameters.ContainsKey(baseParamName))
                {
                    return baseParamName;
                }

                int suffix = 1;
                string paramName;
                do
                {
                    paramName = $"@{columnName}{suffix}";
                    suffix++;
                }
                while (_parameters.ContainsKey(paramName));

                return paramName;
            }

            private static string GetMemberName(Expression expression)
            {
                string? name;
                switch (expression)
                {
                    case MemberExpression member:
                        name = member.Member.GetCustomAttribute<ColumnAttribute>()?.Name ?? member.Member.Name;
                        return name;

                    case UnaryExpression unary when unary.Operand is MemberExpression:
                        name = ((MemberExpression)unary.Operand).Member.GetCustomAttribute<ColumnAttribute>()?.Name ??
                               ((MemberExpression)unary.Operand).Member.Name;
                        return name;

                    case BinaryExpression binary:
                        return GetMemberName(binary.Left);

                    case LambdaExpression lambda:
                        return GetMemberName(lambda.Body);

                    default:
                        throw new NotSupportedException($"Unsupported expression type for member name: {expression.NodeType}");
                }
            }

            /// <summary>
            /// 从表达式中获取实际值（用于参数化查询）
            /// </summary>
            private static object? GetValueFromExpression(Expression expression)
            {
                switch (expression)
                {
                    case ConstantExpression constant:
                        return constant.Value;

                    case MemberExpression member:
                        var objectMember = Expression.Convert(member, typeof(object));
                        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                        var getter = getterLambda.Compile();
                        return getter();

                    case UnaryExpression unary when unary.NodeType == ExpressionType.Convert:
                        return GetValueFromExpression(unary.Operand);

                    default:
                        try
                        {
                            var lambda = Expression.Lambda(expression);
                            var compiled = lambda.Compile();
                            return compiled.DynamicInvoke();
                        }
                        catch (Exception ex)
                        {
                            throw new NotSupportedException(
                                $"Unsupported expression type for value extraction: {expression.NodeType}", ex);
                        }
                }
            }

            private void SetDefaultOrder()
            {
                var defaultSortProperty = typeof(TEntity).GetProperties()
                    .FirstOrDefault(p => p.GetCustomAttribute<DefaultSortNameAttribute>() != null);

                if (defaultSortProperty != null)
                {
                    var columnAttr = defaultSortProperty.GetCustomAttribute<ColumnAttribute>();
                    _orderBy = columnAttr?.Name ?? defaultSortProperty.Name;
                }
            }

            private DynamicParameters ConvertToDynamicParameters()
            {
                var parameters = new DynamicParameters();

                foreach (var kvp in _parameters)
                {
                    parameters.Add(kvp.Key, kvp.Value);
                }

                return parameters;
            }
        }

        public class PagedResult<T>
        {
            public IEnumerable<T> Values { get; set; } = new List<T>();

            public int Page { get; set; }

            public int ItemsPerPage { get; set; }

            public long TotalItems { get; set; }
        }

        /// <summary>
        /// Cursor-based pagination result (for large datasets)
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        public class CursorPagedResult<T>
        {
            /// <summary>
            /// List of entities in current page
            /// </summary>
            public IEnumerable<T> Values { get; set; } = new List<T>();

            /// <summary>
            /// Number of items per page
            /// </summary>
            public int ItemsPerPage { get; set; }

            /// <summary>
            /// Whether there is a next page
            /// </summary>
            public bool HasNextPage { get; set; }

            /// <summary>
            /// Cursor value for next page (null if no next page)
            /// </summary>
            public object? NextCursor { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Defines the name of a table to use in Dapper.Contrib commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Creates a table mapping to a specific name for Dapper.Contrib commands
        /// </summary>
        /// <param name="tableName">The name of this table in the database.</param>
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        /// <summary>
        /// The name of the table in the database
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Specifies that this field is a primary key in the database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies that this field is an explicitly set primary key in the database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ExplicitKeyAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies whether a field is writable in the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class WriteAttribute : Attribute
    {
        /// <summary>
        /// Specifies whether a field is writable in the database.
        /// </summary>
        /// <param name="write">Whether a field is writable in the database.</param>
        public WriteAttribute(bool write)
        {
            Write = write;
        }

        /// <summary>
        /// Whether a field is writable in the database.
        /// </summary>
        public bool Write { get; }
    }

    /// <summary>
    /// Specifies that this is a computed column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ComputedAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies that this is a ignored column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreUpdateAttribute : Attribute
    {
    }

    /// <summary>
    /// Map to the name of column name to use in Dapper.Contrib commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Creates a field mapping to a specific name for Dapper.Contrib commands.
        /// </summary>
        /// <param name="name">The name of a table's column in the database</param>
        public ColumnAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultSortNameAttribute : Attribute
    {

    }
}

/// <summary>
/// The interface for all Dapper.Contrib database operations
/// Implementing this is each provider's model.
/// </summary>
public partial interface ISqlAdapter
{
    /// <summary>
    /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
    /// </summary>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <param name="commandTimeout">The command timeout to use.</param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="columnList">The columns to set with this insert.</param>
    /// <param name="parameterList">The parameters to set for this insert.</param>
    /// <param name="keyProperties">The key columns in this table.</param>
    /// <param name="entityToInsert">The entity to insert.</param>
    /// <returns>The Id of the row created.</returns>
    int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert);

    /// <summary>
    /// Adds the name of a column.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    void AppendColumnName(StringBuilder sb, string columnName);

    /// <summary>
    /// Convert the name of column to adapt db.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    string ConvertColumnName(string columnName);

    /// <summary>
    /// Adds a column equality to a parameter.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    void AppendColumnNameEqualsValue(StringBuilder sb, PropertyInfo property);

    /// <summary>
    /// Retrieve the current paginated data based on the sorted column names.
    /// </summary>
    /// <returns></returns>
    IEnumerable<dynamic> RetrieveCurrentPaginatedData(IDbConnection connection, IDbTransaction? transaction, int? commandTimeout, string tableName, string sortingColumnName, int page, int itemsPerPage, string whereClause, DynamicParameters parameters);
}

/// <summary>
/// The SQL Server database adapter.
/// </summary>
public partial class SqlServerAdapter : ISqlAdapter
{
    /// <summary>
    /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
    /// </summary>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <param name="commandTimeout">The command timeout to use.</param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="columnList">The columns to set with this insert.</param>
    /// <param name="parameterList">The parameters to set for this insert.</param>
    /// <param name="keyProperties">The key columns in this table.</param>
    /// <param name="entityToInsert">The entity to insert.</param>
    /// <returns>The Id of the row created.</returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"insert into {tableName} ({columnList}) values ({parameterList});select SCOPE_IDENTITY() id";
        var multi = connection.QueryMultiple(cmd, entityToInsert, transaction, commandTimeout);

        if (keyProperties.Any())
        {
            var first = multi.Read().FirstOrDefault();
            if (first == null || first.id == null) return 0;

            var id = (int)first.id;
            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            if (propertyInfos.Length == 0) return id;

            var idProperty = propertyInfos[0];
            idProperty.SetValue(entityToInsert, Convert.ChangeType(id, idProperty.PropertyType), null);

            return id;
        }

        var result = multi.Read().Count();

        return result;
    }

    /// <summary>
    /// Retrieve current paginated data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="sortingColumnName">Sorting column name, such as timestamp or auto-increment column</param>
    /// <param name="page">Current page index</param>
    /// <param name="itemsPerPage">Items for per page</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerable<dynamic> RetrieveCurrentPaginatedData(IDbConnection connection, IDbTransaction? transaction, int? commandTimeout, string tableName, string sortingColumnName, int page, int itemsPerPage, string whereClause, DynamicParameters parameters)
    {
        // ✅ Simplified: Single query with standard OFFSET FETCH NEXT
        var sql = $@"SELECT * FROM {tableName} 
                     WHERE {whereClause ?? "1=1"} 
                     ORDER BY {sortingColumnName} DESC 
                     OFFSET {(page - 1) * itemsPerPage} ROWS 
                     FETCH NEXT {itemsPerPage} ROWS ONLY";
        
        return connection.Query(sql, parameters, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Adds the name of a column.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}]", columnName);
    }

    /// <summary>
    /// Adds a column equality to a parameter.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, PropertyInfo property)
    {
        sb.AppendFormat("[{0}] = @{1}", property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name, property.Name);
    }

    /// <summary>
    /// Convert the name of a column to adapt db.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string ConvertColumnName(string columnName)
    {
        return string.Format("[{0}]", columnName);
    }
}

/// <summary>
/// The SQL Server Compact Edition database adapter.
/// </summary>
public partial class SqlCeServerAdapter : ISqlAdapter
{
    /// <summary>
    /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
    /// </summary>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <param name="commandTimeout">The command timeout to use.</param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="columnList">The columns to set with this insert.</param>
    /// <param name="parameterList">The parameters to set for this insert.</param>
    /// <param name="keyProperties">The key columns in this table.</param>
    /// <param name="entityToInsert">The entity to insert.</param>
    /// <returns>The Id of the row created.</returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
        var result = connection.Execute(cmd, entityToInsert, transaction, commandTimeout);

        if (keyProperties.Any() && result > 0)
        {
            var r = connection.Query("select @@IDENTITY id", transaction: transaction, commandTimeout: commandTimeout).ToList();

            if (r[0].id == null) return 0;
            var id = (int)r[0].id;

            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            if (propertyInfos.Length == 0) return id;

            var idProperty = propertyInfos[0];
            idProperty.SetValue(entityToInsert, Convert.ChangeType(id, idProperty.PropertyType), null);

            return id;
        }

        return result;
    }

    /// <summary>
    /// Retrieve current paginated data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="sortingColumnName">Sorting column name, such as timestamp or auto-increment column</param>
    /// <param name="page">Current page index</param>
    /// <param name="itemsPerPage">Items for per page</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerable<dynamic> RetrieveCurrentPaginatedData(IDbConnection connection, IDbTransaction? transaction, int? commandTimeout, string tableName, string sortingColumnName, int page, int itemsPerPage, string whereClause, DynamicParameters parameters)
    {
        // ✅ Simplified: Single query with ROW_NUMBER pagination
        var sql = $@"SELECT * FROM (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY {sortingColumnName} DESC) AS RowNum
                        FROM {tableName}
                        WHERE {whereClause ?? "1=1"}
                     ) AS t
                     WHERE RowNum BETWEEN {(page - 1) * itemsPerPage + 1} AND {page * itemsPerPage}";
        
        return connection.Query(sql, parameters, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Adds the name of a column.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}]", columnName);
    }

    /// <summary>
    /// Adds a column equality to a parameter.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, PropertyInfo property)
    {
        sb.AppendFormat("[{0}] = @{1}", property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name, property.Name);
    }

    /// <summary>
    /// Convert the name of a column to adapt db.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string ConvertColumnName(string columnName)
    {
        return string.Format("[{0}]", columnName);
    }
}

/// <summary>
/// The MySQL database adapter.
/// </summary>
public partial class MySqlAdapter : ISqlAdapter
{
    /// <summary>
    /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
    /// </summary>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <param name="commandTimeout">The command timeout to use.</param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="columnList">The columns to set with this insert.</param>
    /// <param name="parameterList">The parameters to set for this insert.</param>
    /// <param name="keyProperties">The key columns in this table.</param>
    /// <param name="entityToInsert">The entity to insert.</param>
    /// <returns>The Id of the row created.</returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList})";
        var result = connection.Execute(cmd, entityToInsert, transaction, commandTimeout);

        if (keyProperties.Any() && result > 0)
        {
            var r = connection.Query("Select LAST_INSERT_ID() id", transaction: transaction, commandTimeout: commandTimeout);

            var id = r.First().id;
            if (id == null) return 0;
            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            if (propertyInfos.Length == 0) return Convert.ToInt32(id);

            var idp = propertyInfos[0];
            idp.SetValue(entityToInsert, Convert.ChangeType(id, idp.PropertyType), null);

            return Convert.ToInt32(id);
        }

        return result;
    }

    /// <summary>
    /// Retrieve current paginated data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="sortingColumnName">Sorting column name, such as timestamp or auto-increment column</param>
    /// <param name="page">Current page index</param>
    /// <param name="itemsPerPage">Items for per page</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerable<dynamic> RetrieveCurrentPaginatedData(IDbConnection connection, IDbTransaction? transaction, int? commandTimeout, string tableName, string sortingColumnName, int page, int itemsPerPage, string whereClause, DynamicParameters parameters)
    {
        // ✅ Simplified: Single query with standard LIMIT OFFSET
        var sql = $@"SELECT * FROM {tableName} 
                     WHERE {whereClause ?? "1=1"} 
                     ORDER BY `{sortingColumnName}` DESC 
                     LIMIT {itemsPerPage} OFFSET {(page - 1) * itemsPerPage}";
        
        return connection.Query(sql, parameters, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Adds the name of a column.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("`{0}`", columnName);
    }

    /// <summary>
    /// Adds a column equality to a parameter.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, PropertyInfo property)
    {
        sb.AppendFormat("`{0}` = @{1}", property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name, property.Name);
    }

    /// <summary>
    /// Convert the name of a column to adapt db.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string ConvertColumnName(string columnName)
    {
        return string.Format("`{0}`", columnName);
    }
}

/// <summary>
/// The Postgres database adapter.
/// </summary>
public partial class PostgresAdapter : ISqlAdapter
{
    /// <summary>
    /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
    /// </summary>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <param name="commandTimeout">The command timeout to use.</param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="columnList">The columns to set with this insert.</param>
    /// <param name="parameterList">The parameters to set for this insert.</param>
    /// <param name="keyProperties">The key columns in this table.</param>
    /// <param name="entityToInsert">The entity to insert.</param>
    /// <returns>The Id of the row created.</returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("insert into {0} ({1}) values ({2})", tableName, columnList, parameterList);

        // If no primary key then safe to assume a join table with not too much data to return
        var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
        if (propertyInfos.Length == 0)
        {
            sb.Append(" RETURNING *");
        }
        else
        {
            sb.Append(" RETURNING ");
            var first = true;
            foreach (var property in propertyInfos)
            {
                if (!first)
                    sb.Append(", ");
                first = false;
                sb.Append(property.Name);
            }
        }

        var results = connection.Query(sb.ToString(), entityToInsert, transaction, commandTimeout: commandTimeout).ToList();

        if (keyProperties.Any())
        {
            // Return the key by assigning the corresponding property in the object - by product is that it supports compound primary keys
            var id = 0;
            foreach (var p in propertyInfos)
            {
                var value = ((IDictionary<string, object>)results[0])[p.Name.ToLower()];
                p.SetValue(entityToInsert, value, null);
                if (id == 0)
                    id = Convert.ToInt32(value);
            }
            return id;
        }

        return results.Count;
    }

    /// <summary>
    /// Retrieve current paginated data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="sortingColumnName">Sorting column name, such as timestamp or auto-increment column</param>
    /// <param name="page">Current page index</param>
    /// <param name="itemsPerPage">Items for per page</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerable<dynamic> RetrieveCurrentPaginatedData(IDbConnection connection, IDbTransaction? transaction, int? commandTimeout, string tableName, string sortingColumnName, int page, int itemsPerPage, string whereClause, DynamicParameters parameters)
    {
        // ✅ Simplified: Single query with standard LIMIT OFFSET (PostgresAdapter)
        var sql = $@"SELECT * FROM {tableName} 
                     WHERE {whereClause ?? "1=1"} 
                     ORDER BY {sortingColumnName} DESC 
                     LIMIT {itemsPerPage} OFFSET {(page - 1) * itemsPerPage}";
        
        return connection.Query(sql, parameters, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Adds the name of a column.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\"", columnName);
    }

    /// <summary>
    /// Adds a column equality to a parameter.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, PropertyInfo property)
    {
        sb.AppendFormat("\"{0}\" = @{1}", property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name, property.Name);
    }

    /// <summary>
    /// Convert the name of a column to adapt db.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string ConvertColumnName(string columnName)
    {
        return string.Format("\"{0}\"", columnName);
    }
}

/// <summary>
/// The SQLite database adapter.
/// </summary>
public partial class SQLiteAdapter : ISqlAdapter
{
    /// <summary>
    /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
    /// </summary>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <param name="commandTimeout">The command timeout to use.</param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="columnList">The columns to set with this insert.</param>
    /// <param name="parameterList">The parameters to set for this insert.</param>
    /// <param name="keyProperties">The key columns in this table.</param>
    /// <param name="entityToInsert">The entity to insert.</param>
    /// <returns>The Id of the row created.</returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList}); SELECT last_insert_rowid() id";
        var multi = connection.QueryMultiple(cmd, entityToInsert, transaction, commandTimeout);

        if (keyProperties.Any())
        {
            var id = (int)multi.Read().First().id;
            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            if (propertyInfos.Length == 0) return id;

            var idProperty = propertyInfos[0];
            idProperty.SetValue(entityToInsert, Convert.ChangeType(id, idProperty.PropertyType), null);

            return id;
        }

        var result = multi.Read().Count();

        return result;
    }

    /// <summary>
    /// Retrieve current paginated data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="sortingColumnName">Sorting column name, such as timestamp or auto-increment column</param>
    /// <param name="page">Current page index</param>
    /// <param name="itemsPerPage">Items for per page</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerable<dynamic> RetrieveCurrentPaginatedData(IDbConnection connection, IDbTransaction? transaction, int? commandTimeout, string tableName, string sortingColumnName, int page, int itemsPerPage, string whereClause, DynamicParameters parameters)
    {
        // ✅ Simplified: Single query with standard LIMIT OFFSET (SQLiteAdapter)
        var sql = $@"SELECT * FROM {tableName} 
                     WHERE {whereClause ?? "1=1"} 
                     ORDER BY {sortingColumnName} DESC 
                     LIMIT {itemsPerPage} OFFSET {(page - 1) * itemsPerPage}";
        
        return connection.Query(sql, parameters, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Adds the name of a column.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\"", columnName);
    }

    /// <summary>
    /// Adds a column equality to a parameter.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, PropertyInfo property)
    {
        sb.AppendFormat("\"{0}\" = @{1}", property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name, property.Name);
    }

    /// <summary>
    /// Convert the name of a column to adapt db.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string ConvertColumnName(string columnName)
    {
        return string.Format("\"{0}\"", columnName);
    }
}

/// <summary>
/// The Firebase SQL adapter.
/// </summary>
public partial class FbAdapter : ISqlAdapter
{
    /// <summary>
    /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
    /// </summary>
    /// <param name="connection">The connection to use.</param>
    /// <param name="transaction">The transaction to use.</param>
    /// <param name="commandTimeout">The command timeout to use.</param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="columnList">The columns to set with this insert.</param>
    /// <param name="parameterList">The parameters to set for this insert.</param>
    /// <param name="keyProperties">The key columns in this table.</param>
    /// <param name="entityToInsert">The entity to insert.</param>
    /// <returns>The Id of the row created.</returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList})";
        var result = connection.Execute(cmd, entityToInsert, transaction, commandTimeout);

        if (keyProperties.Any())
        {
            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            var keyName = propertyInfos[0].Name;
            var r = connection.Query($"SELECT FIRST 1 {keyName} ID FROM {tableName} ORDER BY {keyName} DESC", transaction: transaction, commandTimeout: commandTimeout);

            var id = r.First().ID;
            if (id == null) return 0;
            if (propertyInfos.Length == 0) return Convert.ToInt32(id);

            var idp = propertyInfos[0];
            idp.SetValue(entityToInsert, Convert.ChangeType(id, idp.PropertyType), null);

            return Convert.ToInt32(id);
        }

        return result;
    }

    /// <summary>
    /// Retrieve current paginated data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName">The table to insert into.</param>
    /// <param name="sortingColumnName">Sorting column name, such as timestamp or auto-increment column</param>
    /// <param name="page">Current page index</param>
    /// <param name="itemsPerPage">Items for per page</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerable<dynamic> RetrieveCurrentPaginatedData(IDbConnection connection, IDbTransaction? transaction, int? commandTimeout, string tableName, string sortingColumnName, int page, int itemsPerPage, string whereClause, DynamicParameters parameters)
    {
        // ✅ Simplified: Single query with Firebird ROWS syntax
        var sql = $@"SELECT * FROM {tableName} 
                     WHERE {whereClause ?? "1=1"} 
                     ORDER BY {sortingColumnName} DESC 
                     ROWS {(page - 1) * itemsPerPage + 1} TO {page * itemsPerPage}";
        
        return connection.Query(sql, parameters, transaction, commandTimeout: commandTimeout);
    }

    /// <summary>
    /// Adds the name of a column.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("{0}", columnName);
    }

    /// <summary>
    /// Adds a column equality to a parameter.
    /// </summary>
    /// <param name="sb">The string builder  to append to.</param>
    /// <param name="columnName">The column name.</param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, PropertyInfo property)
    {
        sb.AppendFormat("{0} = @{1}", property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name, property.Name);
    }

    /// <summary>
    /// Convert the name of a column to adapt db.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string ConvertColumnName(string columnName)
    {
        return string.Format("{0}", columnName);
    }
}
