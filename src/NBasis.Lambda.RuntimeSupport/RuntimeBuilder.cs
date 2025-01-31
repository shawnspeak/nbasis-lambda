using Amazon.Lambda.Core;
using System.Reflection;

namespace NBasis.Lambda.RuntimeSupport
{
    /// <summary>
    /// Fluent builder to create a lambda runtime
    /// </summary>
    public class RuntimeBuilder
    {
        readonly Dictionary<string, Type> _typeTable = [];

        readonly ILambdaSerializer _serializer;

        private RuntimeBuilder(ILambdaSerializer serializer)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// Start the fluent build by setting the serializer
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static RuntimeBuilder SetSerializer(ILambdaSerializer serializer)
        {
            return new RuntimeBuilder(serializer);
        }

        /// <summary>
        /// Add function types to the builder
        /// </summary>
        /// <typeparam name="TFunction"></typeparam>
        /// <param name="nameOverride"></param>
        /// <returns></returns>
        public RuntimeBuilder Add<TFunction>(string nameOverride = null) where TFunction : class
        {
            var type = typeof(TFunction);
            this._typeTable[(nameOverride ?? type.Name).ToLower()] = type;
            return this;
        }

        /// <summary>
        /// Build the function runtime
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public IFunctionRuntime Build(string[] args)
        {
            // check args
            if (args == null) throw new ArgumentNullException(nameof(args), "Missing function name argument");

            var functionName = args[0].ToLower();
            if (!this._typeTable.ContainsKey(functionName))
                throw new ArgumentException("Function not found");

            // resolve function type
            var functionType = this._typeTable[functionName];

            // determine event, request or multi type
            var eventType = GetTypeForEventFunction(functionType);
            if (eventType != null)
            {
                // return event runtime
                var functionInstance = Activator.CreateInstance(functionType);
                var runtimeType = typeof(EventFunctionRuntime<>).MakeGenericType(eventType);
                return (IFunctionRuntime)Activator.CreateInstance(runtimeType, functionInstance, _serializer);
            }
            
            var requestTypes = GetTypesForRequestFunction(functionType);
            if (requestTypes?.Length == 2)
            {
                // return request runtime
                var functionInstance = Activator.CreateInstance(functionType);
                var runtimeType = typeof(RequestFunctionRuntime<,>).MakeGenericType(requestTypes[0], requestTypes[1]);
                return (IFunctionRuntime)Activator.CreateInstance(runtimeType, functionInstance, _serializer);
            }

            if (IsMultiEvent(functionType))
            {
                // return multi-event runtime
                var functionInstance = Activator.CreateInstance(functionType);
                var runtimeType = typeof(MultiEventFunctionRuntime);
                return (IFunctionRuntime)Activator.CreateInstance(runtimeType, functionInstance, _serializer);
            }

            throw new ApplicationException("Function type does not implement ILambdaRequestFunction or ILambdaEventFunction or inherit from MultiEventFunction");
        }

        private static bool IsMultiEvent(Type functionType)
        {
            return functionType.GetTypeInfo().IsSubclassOf(typeof(MultiEventFunction));
        }

        private static Type[] GetTypesForRequestFunction(Type functionType)
        {
            return functionType.GetTypeInfo().ImplementedInterfaces
                    .FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ILambdaRequestFunction<,>))?
                    .GetTypeInfo().GenericTypeArguments.ToArray();
        }

        private static Type GetTypeForEventFunction(Type functionType)
        {
            return functionType.GetTypeInfo().ImplementedInterfaces
                    .FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ILambdaEventFunction<>))?
                    .GetTypeInfo()
                    .GenericTypeArguments
                    .FirstOrDefault();
        }
    }
}
