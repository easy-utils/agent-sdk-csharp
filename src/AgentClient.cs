using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyRpc;
using Easyrpc.Agent.V1;

namespace Agentsdk
{
    /// <summary>AgentRpcError maps a Connect code to a typed result.</summary>
    public class AgentException : Exception
    {
        public int Code { get; }
        public AgentException(int code, string message) : base($"agent: code={code} {message}") { Code = code; }
    }

    /// <summary>
    /// Strong-typed agent client: generated AgentServiceClient over an EasyRpc
    /// HttpClientTransport, with a Bearer token attached to every request.
    /// </summary>
    public class AgentClient
    {
        private readonly AgentServiceClient _rpc;
        public AgentServiceClient Client => _rpc;

        public AgentClient(string baseUrl, string token = "", Transport? transport = null)
        {
            var t = transport ?? HttpClientTransport.H2(baseUrl);
            _rpc = token.Length == 0
                ? new AgentServiceClient(t)
                : new AgentServiceClient(new AuthTransport(t, $"Bearer {token}"));
        }
    }

    /// <summary>Adds an Authorization header to every outbound request.</summary>
    public class AuthTransport : Transport
    {
        private readonly Transport _inner;
        private readonly string _value;
        public AuthTransport(Transport inner, string value) { _inner = inner; _value = value; }

        public Task<Response> Send(Request req)
        {
            req.Headers["Authorization"] = new List<string> { _value };
            return _inner.Send(req);
        }

        public async Task<IAsyncEnumerable<byte[]>> OpenStream(Request req)
        {
            req.Headers["Authorization"] = new List<string> { _value };
            return await _inner.OpenStream(req);
        }
    }
}
