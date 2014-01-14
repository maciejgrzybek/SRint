//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: protobuf.proto
namespace protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"Message")]
  public partial class Message : global::ProtoBuf.IExtensible
  {
    public Message() {}
    
    private Message.MessageType _type;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"type", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public Message.MessageType type
    {
      get { return _type; }
      set { _type = value; }
    }
    private Message.Election _election_content = null;
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"election_content", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public Message.Election election_content
    {
      get { return _election_content; }
      set { _election_content = value; }
    }
    private Message.State _state_content = null;
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"state_content", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public Message.State state_content
    {
      get { return _state_content; }
      set { _state_content = value; }
    }
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"Election")]
  public partial class Election : global::ProtoBuf.IExtensible
  {
    public Election() {}
    
    private long _timestamp;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"timestamp", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long timestamp
    {
      get { return _timestamp; }
      set { _timestamp = value; }
    }
    private int _state_id;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"state_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int state_id
    {
      get { return _state_id; }
      set { _state_id = value; }
    }
    private int _node_id;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"node_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int node_id
    {
      get { return _node_id; }
      set { _node_id = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"State")]
  public partial class State : global::ProtoBuf.IExtensible
  {
    public State() {}
    
    private long _state_id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"state_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long state_id
    {
      get { return _state_id; }
      set { _state_id = value; }
    }
    private readonly global::System.Collections.Generic.List<Message.NodeDescription> _nodes = new global::System.Collections.Generic.List<Message.NodeDescription>();
    [global::ProtoBuf.ProtoMember(2, Name=@"nodes", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<Message.NodeDescription> nodes
    {
      get { return _nodes; }
    }
  
    private readonly global::System.Collections.Generic.List<Message.Variable> _variables = new global::System.Collections.Generic.List<Message.Variable>();
    [global::ProtoBuf.ProtoMember(3, Name=@"variables", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<Message.Variable> variables
    {
      get { return _variables; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"NodeDescription")]
  public partial class NodeDescription : global::ProtoBuf.IExtensible
  {
    public NodeDescription() {}
    
    private string _ip;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"ip", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string ip
    {
      get { return _ip; }
      set { _ip = value; }
    }
    private int _port;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"port", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int port
    {
      get { return _port; }
      set { _port = value; }
    }
    private int _node_id;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"node_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int node_id
    {
      get { return _node_id; }
      set { _node_id = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"Variable")]
  public partial class Variable : global::ProtoBuf.IExtensible
  {
    public Variable() {}
    
    private string _name;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"name", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string name
    {
      get { return _name; }
      set { _name = value; }
    }
    private long _value;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"value", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public long value
    {
      get { return _value; }
      set { _value = value; }
    }
    private readonly global::System.Collections.Generic.List<Message.NodeDescription> _owners = new global::System.Collections.Generic.List<Message.NodeDescription>();
    [global::ProtoBuf.ProtoMember(4, Name=@"owners", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<Message.NodeDescription> owners
    {
      get { return _owners; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    [global::ProtoBuf.ProtoContract(Name=@"MessageType")]
    public enum MessageType
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"STATE", Value=0)]
      STATE = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"ELECTION", Value=1)]
      ELECTION = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"ENTRY_REQUEST", Value=2)]
      ENTRY_REQUEST = 2
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}