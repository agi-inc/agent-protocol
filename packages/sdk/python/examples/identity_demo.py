"""
Example: Agent Identity Protocol with Agent Protocol SDK.

Self-contained — imports only the identity module (stdlib only).
No SDK dependencies required.

Run:
    python3 packages/sdk/python/examples/identity_demo.py

Demonstrates:
1. Creating agent identities with cryptographic keypairs
2. Signing and verifying handoff tokens
3. Task attestation chains
4. Tamper detection
"""
import sys
import os

# Import identity module directly (stdlib only, no SDK deps needed)
sys.path.insert(0, os.path.join(os.path.dirname(__file__), "..", "agent_protocol"))
from identity import AgentIdentity, HandoffToken


def main():
    print("=" * 60)
    print("Agent Identity Protocol — Agent Protocol SDK Demo")
    print("=" * 60)

    # 1. Create identities
    researcher = AgentIdentity.create("research-agent")
    writer = AgentIdentity.create("writer-agent")

    print(f"\n🔑 {researcher.agent_id}")
    print(f"   Fingerprint: {researcher.fingerprint()}")
    print(f"   Public Key:  {researcher.public_key[:40]}...")

    print(f"\n🔑 {writer.agent_id}")
    print(f"   Fingerprint: {writer.fingerprint()}")
    print(f"   Public Key:  {writer.public_key[:40]}...")

    # 2. Sign a handoff token
    token = HandoffToken(
        from_agent=researcher.agent_id,
        to_agent=writer.agent_id,
        task_id="task-001",
        context_hash="abc123def456",
    )
    signature = token.sign(researcher)

    print(f"\n🤝 Handoff: {token.from_agent} → {token.to_agent}")
    print(f"   Task: {token.task_id}")
    print(f"   Signature: {signature[:40]}...")

    # 3. Verify the handoff
    is_valid = token.verify(researcher, signature)
    print(f"   Verified: {'✅' if is_valid else '❌'}")

    # 4. Tamper detection
    token.context_hash = "TAMPERED"
    is_valid = token.verify(researcher, signature)
    print(f"   Tampered token verified: {'✅ (BUG!)' if is_valid else '❌ (correctly rejected)'}")

    # 5. HTTP headers for Agent Protocol requests
    print(f"\n📡 Outbound headers:")
    for k, v in researcher.to_header().items():
        print(f"   {k}: {v}")

    print(f"\n{'=' * 60}")
    print("For Ed25519 production signing:")
    print("  pip install works-with-agents")
    print("  https://workswithagents.com/specs/identity.md")
    print("=" * 60)


if __name__ == "__main__":
    main()
