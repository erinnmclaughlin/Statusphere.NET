## Quick Background
**MST (Merkle Search Tree** is the internal data structure ATP uses to store a user’s repo. 
Each MST node is an object with tiny field names (like `e`, `k`, `p`, `t`, `v`, `l`) to reduce size.

An MST node can have:
* `e` (“entries”): An array of objects, each describing a sub-tree or a record leaf.
  * `k`: Base64-encoded key (like "YXBwLmJza3kuZ3JhcGguZm9sbG93Lz...") which decodes to something like "app.bsky.graph.follow/3l...".
  * `p`: Prefix length used internally.
  * `t`: A CID for a sub-tree (child MST node).
  * `v`: A CID for the “value” (often the actual record).
* `l`: A CID for the “link” to a sibling or next node.

You can think of an MST node as a “partial key-value tree,” where:
* If an entry has `t`, that points to _another MST node_.
* If an entry has `v`, that points to the _actual record_ (or sometimes another child, depending on how the tree is balanced).
* `l` is another MST node to link deeper.

