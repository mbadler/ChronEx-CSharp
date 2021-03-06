﻿using ChronEx.Models.AST;
using System;

/// <summary>
/// Container element is the parent class for all elements that contain other elements
/// they act as a pipeline accepting requests on behalf ot their elemets 
/// </summary>
public abstract class ContainerElement : ElementBase
{
    public ElementBase ContainedElement { get; set; }

    //statndard implementation just assigns it to ContainedElement
    internal virtual void AddContainedElement(ElementBase existingElement)
    {
        ContainedElement = existingElement;
    }

    
}
