using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ContentPipeline
{
	public class BoundingNode
	{
		BoundingSphere boundingSphere;
		List<BoundingNode> children;

		public BoundingNode(BoundingSphere boundingSphere, List<BoundingNode> children)
		{
			this.boundingSphere = boundingSphere;
			this.children = children;
		}

		public void AddChildren(BoundingNode child)
		{
			children.Add(child);
		}

		public void AddBoundingSphere()
		{
		}
	}
}
