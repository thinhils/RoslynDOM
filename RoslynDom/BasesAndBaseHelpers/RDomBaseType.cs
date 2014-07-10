﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using RoslynDom.Common;

namespace RoslynDom
{
    public abstract class RDomBaseType<T, TSyntax>
        : RDomBase<T, TSyntax, INamedTypeSymbol>, IType<T>, IRDomTypeMemberContainer
        where TSyntax : SyntaxNode
        where T : class, IType<T>
    {
        private IList<ITypeMember> _members = new List<ITypeMember>();
        private MemberKind _memberKind;
        private StemMemberKind _stemMemberKind;
        private IList<ITypeParameter> _typeParameters = new List<ITypeParameter>();

        internal RDomBaseType(
            TSyntax rawItem,
            MemberKind memberKind,
            StemMemberKind stemMemberKind,
            IEnumerable<ITypeMember> members,
            params PublicAnnotation[] publicAnnotations)
            : base(rawItem, publicAnnotations)
        {
            foreach (var member in members)
            { AddOrMoveMember(member); }
            _memberKind = memberKind;
            _stemMemberKind = stemMemberKind;
        }

        internal RDomBaseType(T oldIDom)
             : base(oldIDom)
        {
            var oldRDom = oldIDom as RDomBaseType<T, TSyntax>;
            _memberKind = oldRDom._memberKind;
            _stemMemberKind = oldRDom._stemMemberKind;
            AccessModifier = oldRDom.AccessModifier;
            var newMembers = RoslynDomUtilities.CopyMembers(oldRDom._members);
            foreach (var member in newMembers)
            { AddOrMoveMember(member); }
            var newTypeParameters = RoslynDomUtilities.CopyMembers(oldRDom._typeParameters);
            foreach (var typeParameter in newTypeParameters)
            { AddOrMoveTypeParameter(typeParameter); }
        }

        protected override void Initialize()
        {
            base.Initialize();
            AccessModifier = (AccessModifier)Symbol.DeclaredAccessibility;
            var newTypeParameters = this.TypedSymbol.TypeParametersFrom();
            foreach (var typeParameter in newTypeParameters)
            { AddOrMoveTypeParameter(typeParameter); }
        }

        public void RemoveMember(ITypeMember member)
        {
            if (member.Parent == null)
            { _members.Remove(member); }
            else
            { RoslynDomUtilities.RemoveMemberFromParent(this, member); }
        }

         public void AddOrMoveMember(ITypeMember member)
        {
            RoslynDomUtilities.PrepMemberForAdd(this, member);
            _members.Add(member);
        }

        public void ClearMembers()
        { _members.Clear(); }

        public void RemoveTypeParameter(ITypeParameter typeParameter)
        { _typeParameters.Remove(typeParameter); }

        public void AddOrMoveTypeParameter(ITypeParameter typeParameter)
        { _typeParameters.Add(typeParameter); }

        public void ClearTypeParameters()
        { _typeParameters.Clear(); }

        public string Namespace
        { get { return RoslynDomUtilities.GetNamespace(this.Parent); } }

        public string QualifiedName
        { get { return GetQualifiedName(); } }

        public IEnumerable<ITypeParameter> TypeParameters
        {
            get
            {
                return _typeParameters;
            }
        }

        public IEnumerable<ITypeMember> Members
        { get { return _members; } }

        public IEnumerable<IMethod> Methods
        { get { return Members.OfType<IMethod>(); } }

        public IEnumerable<IProperty> Properties
        { get { return Members.OfType<IProperty>(); } }

        public IEnumerable<IField> Fields
        { get { return Members.OfType<IField>(); } }

        public IEnumerable<IAttribute> Attributes
        { get { return GetAttributes(); } }

        public AccessModifier AccessModifier { get; set; }

        public MemberKind MemberKind
        { get { return _memberKind; } }

        public StemMemberKind StemMemberKind
        { get { return _stemMemberKind; } }
    }
}
