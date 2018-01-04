using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpressionTargets : PropertyAttribute {

	public readonly string[] names;
	public ExpressionTargets(string[] names) {
		this.names = names;
	}
}
