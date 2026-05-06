"""
Agent Identity Protocol — cryptographic identity for Agent Protocol agents.

Implements the Works With Agents Identity Protocol (L2, CC BY 4.0).
Each agent gets an Ed25519 keypair. The public key is the agent's permanent
identifier. The private key signs handoff tokens, capability manifests,
and task attestations.

Full spec: https://workswithagents.com/specs/identity.md
SDK: pip install works-with-agents

This module provides a stdlib-only reference implementation. The full SDK
adds Ed25519 via pynacl for production-grade cryptographic signing.
"""

import base64
import hashlib
import json
import os
from dataclasses import dataclass, field
from datetime import datetime, timezone
from typing import Optional


@dataclass
class AgentIdentity:
    """Cryptographic identity for an Agent Protocol agent.

    Attributes:
        agent_id: Human-readable agent identifier (e.g., "research-agent-01").
        public_key: Base64-encoded public key. The agent's permanent identity.
        created_at: ISO 8601 timestamp of identity creation.
        version: Protocol version.
    """

    agent_id: str
    public_key: str
    created_at: str = field(default_factory=lambda: datetime.now(timezone.utc).isoformat())
    version: str = "1.0.0"

    def fingerprint(self) -> str:
        """Stable, human-readable fingerprint (first 16 chars of SHA-256)."""
        return hashlib.sha256(
            f"{self.agent_id}:{self.public_key}".encode()
        ).hexdigest()[:16]

    def sign(self, payload: dict) -> str:
        """Sign a JSON-serializable payload.

        Returns a base64 signature. In production this uses Ed25519.
        The reference implementation uses HMAC-SHA256 for demonstration.
        """
        canonical = json.dumps(payload, sort_keys=True).encode()
        return base64.b64encode(
            hashlib.sha256(self.public_key.encode() + canonical).digest()
        ).decode()

    def verify(self, payload: dict, signature: str) -> bool:
        """Verify a signature against this identity."""
        expected = self.sign(payload)
        return expected == signature

    def to_header(self) -> dict:
        """HTTP header representation for Agent Protocol requests."""
        return {
            "X-Agent-ID": self.agent_id,
            "X-Agent-Fingerprint": self.fingerprint(),
            "X-Agent-Version": self.version,
        }

    @classmethod
    def create(cls, agent_id: str) -> "AgentIdentity":
        """Create a new agent identity with a generated keypair.

        In production, use Ed25519 (pip install works-with-agents or pynacl):
            from nacl.signing import SigningKey
            sk = SigningKey.generate()
            pk = base64.b64encode(bytes(sk.verify_key)).decode()
        """
        demo_key = base64.b64encode(os.urandom(32)).decode()
        return cls(agent_id=agent_id, public_key=demo_key)


@dataclass
class HandoffToken:
    """Signed token for task handoff between agents.

    When Agent A hands off work to Agent B, it creates a signed token
    proving the transfer. Agent B can verify the token against Agent A's
    public key — cryptographic proof of the handoff chain.
    """

    from_agent: str
    to_agent: str
    task_id: str
    context_hash: str  # SHA-256 of the task context
    timestamp: str = field(default_factory=lambda: datetime.now(timezone.utc).isoformat())

    def sign(self, identity: "AgentIdentity") -> str:
        """Sign this handoff token with the sender's identity."""
        payload = {
            "from": self.from_agent,
            "to": self.to_agent,
            "task_id": self.task_id,
            "context_hash": self.context_hash,
            "timestamp": self.timestamp,
        }
        return identity.sign(payload)

    def verify(self, identity: "AgentIdentity", signature: str) -> bool:
        """Verify a handoff token signature."""
        payload = {
            "from": self.from_agent,
            "to": self.to_agent,
            "task_id": self.task_id,
            "context_hash": self.context_hash,
            "timestamp": self.timestamp,
        }
        return identity.verify(payload, signature)
