using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Google.ProtocolBuffers;
using KRPC.Schema.KRPC;
using KRPC.Utils;

namespace KRPC.Service
{
    class ServiceSignature
    {
        public string Name { get; private set; }

        public Dictionary<string,ProcedureSignature> Procedures { get; private set; }

        public ServiceSignature (Type type)
        {
            Name = type.Name;
            var procedureTypes = Reflection.GetMethodsWith<KRPCProcedure> (type);
            try {
                Procedures = procedureTypes
                    .Select (x => new ProcedureSignature (type.Name, x))
                    .ToDictionary (x => x.Name);
            } catch (ArgumentException) {
                // Handle procedure name clashes
                // TODO: move into Utils
                var duplicates = procedureTypes
                    .Select (x => x.Name)
                    .GroupBy (x => x)
                    .Where (group => group.Count () > 1)
                    .Select (group => group.Key)
                    .ToArray ();
                throw new ServiceException (
                    "Service " + Name + " contains duplicate Procedures, " +
                    "and overloading is not permitted. " +
                    "Duplicates are " + String.Join (", ", duplicates));
            }
            if (Procedures.Count == 0)
                throw new ServiceException ("Service " + Name + " does not contain any Procedures");
        }
    }
}
