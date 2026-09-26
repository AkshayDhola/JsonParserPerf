using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace JsonParserPerf.Protobuf;

public static class ProtobufTypeRegistry
{
    private const string DescriptorPropertyName = "Descriptor";

    private static readonly Lazy<TypeRegistry> RegistryCache =
        new(GetRegistryFromAssemblies, LazyThreadSafetyMode.ExecutionAndPublication);
    
    public static TypeRegistry FromLoadedAssemblies()
    {
        return RegistryCache.Value;
    }
    
    private static TypeRegistry GetRegistryFromAssemblies()
    {
        var files = new HashSet<FileDescriptor>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in GetTypesSafely(assembly))
            {
                if (type is null || type.IsAbstract || !typeof(IMessage).IsAssignableFrom(type))
                {
                    continue;
                }

                // Generated message classes expose their descriptor as a static property.
                if (type.GetProperty(DescriptorPropertyName, BindingFlags.Public | BindingFlags.Static)?.GetValue(null)
                    is MessageDescriptor descriptor)
                {
                    files.Add(descriptor.File);
                }
            }
        }

        return TypeRegistry.FromFiles(files);
    }

    private static Type?[] GetTypesSafely(Assembly assembly)
    {
        try
        {
            if (assembly.IsDynamic)
            {
                return [];
            }

            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types;
        }
        catch (FileNotFoundException)
        {
            return [];
        }
    }
}