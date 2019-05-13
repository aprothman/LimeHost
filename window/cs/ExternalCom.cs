using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

using functionTable = System.Collections.Generic.Dictionary<string, System.Delegate>;

namespace LimeWrapper
{
    public class ExternalCom
    {
        private DllWrapper.ExternalComListener _listener = null;
        private functionTable _externalCalls;
        private int _callCounter = 0;

        public ExternalCom()
        {
            _externalCalls = new functionTable();
            _listener = CallReceived;

            // kickstart the json parser so we don't drag on our first call
            var temp = EncodeReturnValue("temp", -1, "temp");
            var strTemp = temp.ToString();
        }

        public void ExportFunction(string name, Delegate func)
        {
            _externalCalls.Add(name, func);
        }

        public T CallExternalFunction<T>(string name, params object[] args)
        {
            //var callString = EncodeFunction(name, args);
            //var rawRet = DllWrapper.CallExternal(callString);

            //var jsonRet = JObject.Parse(rawRet);
            //if (name != (string)jsonRet["functionName"]) {
            //    throw new Exception("External Com function name mismatches the return");
            //}

            //jsonRet = (JObject)jsonRet["returnValue"];

            //if (typeof(T) != DecodeDataType((string)jsonRet["type"])) {
            //    throw new Exception("External Com return type mismatch");
            //}

            //var retValue = (string)jsonRet["value"];

            //return (T)Convert.ChangeType(retValue, typeof(T));

            throw new NotImplementedException();
        }

        public void CallExternalFunction(string name, params object[] args)
        {
            var callString = EncodeFunction(name, args);
            DllWrapper.CallExternal(callString);
        }

        public void RegisterListener()
        {
            DllWrapper.SetExternalComListener(_listener);
        }

        private void UnregisterListener()
        {
            DllWrapper.SetExternalComListener(null);
        }


        private string EncodeFunction(string name, params object[] args)
        {
            var jsonArgs = new JArray();
            foreach (var arg in args) {
                jsonArgs.Add(BuildArgNode(arg));
            }
            var func = new JObject() {
                { "functionName", name },
                { "externalComCallID", _callCounter++ },
                { "arguments", jsonArgs }
            };

            return func.ToString();
        }

        private string EncodeReturnValue(string name, int id, object arg)
        {
            var argNode = BuildArgNode(arg);
            var ret = new JObject() {
                { "functionName", name },
                { "externalComCallID", id },
                { "returnValue", argNode }
            };

            return ret.ToString();
        }

        private JObject BuildArgNode(object arg)
        {
            switch (Type.GetTypeCode(arg.GetType())) {
                case TypeCode.String:
                    return new JObject() {
                        { "value", arg.ToString() },
                        { "type", "String" }
                    };
                case TypeCode.Empty:
                    return new JObject() {
                        { "value", "null" },
                        { "type", "Null" }
                    };
                case TypeCode.Boolean:
                    return new JObject() {
                        { "value", (bool)arg ? "true" : "false" },
                        { "type", "Bool" }
                    };
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return new JObject() {
                        { "value", (int)arg },
                        { "type", "Int" }
                    };
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return new JObject() {
                        { "value", (double)arg },
                        { "type", "Float" }
                    };
                default:
                    return new JObject();
            }
        }

        private object DecodeArgNode(JObject jsonArg)
        {
            switch ((string)jsonArg["type"]) {
                case "String":
                    return jsonArg["value"].ToString();
                case "Bool":
                    return Convert.ToBoolean(jsonArg["value"].ToString());
                case "Int":
                    return Convert.ToInt32(jsonArg["value"].ToString());
                case "Float":
                    return Convert.ToDouble(jsonArg["value"].ToString());
                default:
                    return null;
            }
        }

        private Type DecodeDataType(string dataType)
        {
            switch (dataType) {
                case "String":
                    return typeof(string);
                case "Bool":
                    return typeof(bool);
                case "Int":
                    return typeof(int);
                case "Float":
                    return typeof(double);
                default:
                    return typeof(void);
            }
        }

        private object[] JsonToArgs(JArray jsonArgs)
        {
            var args = new object[jsonArgs.Count];
            for (var i = 0; i < jsonArgs.Count; ++i) {
                args[i] = DecodeArgNode((JObject)jsonArgs[i]);
            }

            return args;
        }

        /// <summary>
        /// Handle calls from the engine
        /// </summary>
        /// <param name="callString">JSON-encoded rpc, stored as an ansi string</param>
        private void CallReceived(IntPtr callString)
        {
            var strCall = Marshal.PtrToStringAnsi(callString);

            var jsonCall = JObject.Parse(strCall);
            var jsonArgs = (JArray)jsonCall["arguments"];
            var funcName = (string)jsonCall["functionName"];
            var id = (int)jsonCall["externalComCallID"];

            var args = JsonToArgs(jsonArgs);
            var f = _externalCalls[funcName];

            IntPtr ret = IntPtr.Zero;

            f.DynamicInvoke(args);
        }
    }
}
