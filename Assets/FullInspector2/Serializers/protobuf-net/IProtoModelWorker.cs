using ProtoBuf.Meta;

namespace FullInspector {
    public interface IProtoModelWorker {
        void Work(RuntimeTypeModel model);
    }
}