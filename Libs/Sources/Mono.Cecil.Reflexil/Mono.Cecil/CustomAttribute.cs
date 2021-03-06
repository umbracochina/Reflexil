//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;

using Mono.Collections.Generic;

namespace Mono.Cecil {

	public struct CustomAttributeArgument {

		// HACK - Reflexil - Private scope for setters
		private TypeReference type;
		private object value;
		// HACK - Reflexil - Ends

		public TypeReference Type {
			get { return type; }
			// HACK - Reflexil - Setter
            set {
                Mixin.CheckType(value);
                type = value;
            }
			// HACK - Reflexil - Ends
		}

		public object Value {
			get { return value; }
			// HACK - Reflexil - Setter
            set { this.value = value; }
		}

		public CustomAttributeArgument (TypeReference type, object value)
		{
			Mixin.CheckType (type);
			this.type = type;
			this.value = value;
		}
	}

	// HACK - Reflexil - Partial for legacy classes
	public partial struct CustomAttributeNamedArgument {

        readonly string name;
  		// HACK - Reflexil - Private scope for setter
		private CustomAttributeArgument argument;

		public string Name {
			get { return name; }
		}

		public CustomAttributeArgument Argument {
			get { return argument; }
			// HACK - Reflexil - Setter
            set { argument = value; }
		}

		public CustomAttributeNamedArgument (string name, CustomAttributeArgument argument)
		{
			Mixin.CheckName (name);
			this.name = name;
			this.argument = argument;
		}
	}

	public interface ICustomAttribute {

		TypeReference AttributeType { get; }

		bool HasFields { get; }
		bool HasProperties { get; }
		Collection<CustomAttributeNamedArgument> Fields { get; }
		Collection<CustomAttributeNamedArgument> Properties { get; }
	}

	// HACK - Reflexil - Partial for legacy classes
	public sealed partial class CustomAttribute : ICustomAttribute {

		internal CustomAttributeValueProjection projection;
		readonly internal uint signature;
		internal bool resolved;
		MethodReference constructor;
		byte [] blob;
		internal Collection<CustomAttributeArgument> arguments;
		internal Collection<CustomAttributeNamedArgument> fields;
		internal Collection<CustomAttributeNamedArgument> properties;

		public MethodReference Constructor {
			get { return constructor; }
			set { constructor = value; }
		}

		public TypeReference AttributeType {
			get { return constructor.DeclaringType; }
		}

		public bool IsResolved {
			get { return resolved; }
		}

		public bool HasConstructorArguments {
			get {
				Resolve ();

				return !arguments.IsNullOrEmpty ();
			}
		}

		public Collection<CustomAttributeArgument> ConstructorArguments {
			get {
				Resolve ();

				return arguments ?? (arguments = new Collection<CustomAttributeArgument> ());
			}
		}

		public bool HasFields {
			get {
				Resolve ();

				return !fields.IsNullOrEmpty ();
			}
		}

		public Collection<CustomAttributeNamedArgument> Fields {
			get {
				Resolve ();

				return fields ?? (fields = new Collection<CustomAttributeNamedArgument> ());
			}
		}

		public bool HasProperties {
			get {
				Resolve ();

				return !properties.IsNullOrEmpty ();
			}
		}

		public Collection<CustomAttributeNamedArgument> Properties {
			get {
				Resolve ();

				return properties ?? (properties = new Collection<CustomAttributeNamedArgument> ());
			}
		}

		internal bool HasImage {
			get { return constructor != null && constructor.HasImage; }
		}

		internal ModuleDefinition Module {
			get { return constructor.Module; }
		}

		internal CustomAttribute (uint signature, MethodReference constructor)
		{
			this.signature = signature;
			this.constructor = constructor;
			this.resolved = false;
		}

		public CustomAttribute (MethodReference constructor)
		{
			this.constructor = constructor;
			this.resolved = true;
		}

		public CustomAttribute (MethodReference constructor, byte [] blob)
		{
			this.constructor = constructor;
			this.resolved = false;
			this.blob = blob;
		}

		public byte [] GetBlob ()
		{
			if (blob != null)
				return blob;

			if (!HasImage)
				throw new NotSupportedException ();

			return Module.Read (ref blob, this, (attribute, reader) => reader.ReadCustomAttributeBlob (attribute.signature));
		}

		void Resolve ()
		{
			if (resolved || !HasImage)
				return;

			Module.Read (this, (attribute, reader) => {
				try {
					reader.ReadCustomAttributeSignature (attribute);
					resolved = true;
				} catch (ResolutionException) {
					if (arguments != null)
						arguments.Clear ();
					if (fields != null)
						fields.Clear ();
					if (properties != null)
						properties.Clear ();

					resolved = false;
				}
				return this;
			});
		}
	}
}
