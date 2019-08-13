//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Pandora.Client.SystemBackup.Interfaces;

namespace Pandora.Client.SystemBackup
{
    public static class FileBackupProcessor
    {
        public static byte[] GetBackupFileBytes(IBackupObject aObjectToBackup)
        {
            var lArgumentType = aObjectToBackup.GetType();
            if (!lArgumentType.IsSerializable) throw new InvalidOperationException($"Object of type {lArgumentType.Name} must implement serializable attribute to be used in backup by file process");
            var lObjectType = aObjectToBackup.GetType();
            if (!lObjectType.Name.Contains("BackupObject")) throw new TypeLoadException($"Type {lObjectType.Name} must contain the keyword 'BackupObject' in its name");
            var lSerializableProperty = lObjectType.GetProperties().Where(lProperty => typeof(IBackupSerializable).IsAssignableFrom(lProperty.PropertyType)).Select(lProperty => lProperty).FirstOrDefault();
            var lInstanceType = lSerializableProperty.GetValue(aObjectToBackup).GetType();
            if (lSerializableProperty != null && !lInstanceType.Name.Contains("Serializable")) throw new TypeLoadException($"Type {lSerializableProperty.Name} must contain the keyword 'Serializable' in its name");

            var lFormatter = new BinaryFormatter();
            using (MemoryStream lMemStream = new MemoryStream())
            {
                lFormatter.Serialize(lMemStream, aObjectToBackup);
                return lMemStream.ToArray();
            }
        }

        public static T GetRetoreObjectFromBytes<T, U>(byte[] aRestoreFileBytes) where T : IBackupObject, new() where U : IBackupSerializable, new()
        {
            var lFormatter = new BinaryFormatter();
            lFormatter.Binder = new BackupProcessorSerializationBinder<T, U>();
            using (MemoryStream lMemStream = new MemoryStream(aRestoreFileBytes))
            {
                var lResult = (T)lFormatter.Deserialize(lMemStream);
                return lResult;
            }
        }

        private class BackupProcessorSerializationBinder<T, U> : SerializationBinder where T : IBackupObject, new() where U : IBackupSerializable, new()
        {
            public override Type BindToType(string aAssemblyName, string aTypeName)
            {
                var lBaseType = typeof(T);
                var lSerializableType = typeof(U);
                if (aTypeName.Contains("BackupObject")) return lBaseType;
                if (aTypeName.Contains("Serializable")) return lSerializableType;
                return null;
            }
        }
    }
}