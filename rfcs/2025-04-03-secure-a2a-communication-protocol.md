# Secure Agent-to-Agent (A2A) Communication Protocol

| Feature name  | Secure Agent-to-Agent (A2A) Communication Protocol                   |
| :------------ | :------------------------------------------------------------------- |
| **Author(s)** | Jeff Nasseri                                                         |
| **RFC PR:**   |                                                                      |
| **Updated**   | 2025-04-03                                                           |
| **Obsoletes** |                                                                      |

## Summary

This RFC proposes a dedicated secure communication protocol for Agent-to-Agent (A2A) interactions. The protocol enables secure communication between specialized autonomous agents (such as those handling accounting and payment processing) through a dedicated handshake mechanism and encryption framework. Unlike HTTP which is designed for one-way Client-Server communication, this protocol adopts a bidirectional communication model inspired by P2P networks like Bitcoin and Ethereum, where each agent node acts as both client and server simultaneously. This approach provides more freedom than protocols built on HTTP/WebSocket and is better suited for a network of interconnected autonomous agents. This proposal aims to establish a higher-level communication standard beyond traditional HTTP for secure, efficient agent interactions across networks.

## Motivation

As AI agents become more specialized and autonomous, there's an increasing need for secure, direct communication between them. Current web protocols like HTTP were not designed specifically for A2A communication, where specialized agents need to exchange sensitive information (like financial data between accounting and payment systems) with strong security guarantees.

**Problems with existing approaches:**

1. Traditional web protocols lack specialized security measures for agent-specific communications
2. Current solutions often rely on general-purpose encryption not optimized for agent-based workflows
3. There's no standardized handshake procedure for establishing secure A2A channels
4. Overhead from generic protocols reduces efficiency in agent-specific contexts

## Design Proposal

### Core Protocol Structure

The A2A secure communication protocol consists of three main components:

1. **Dedicated A2A Certificate Exchange API**: Separate from main agent functionality, this API handles security credentials
2. **A2A Handshake Protocol**: Initial negotiation to establish encryption parameters and verify agent identities
3. **Secure Communication Channel**: Encrypted data exchange pathway for ongoing communication

### Handshake Protocol

The A2A handshake initiates secure communication through these steps:

1. **Initiation**: Requesting agent sends initial handshake with supported encryption algorithms
2. **Certificate Exchange**: Agents exchange certificates through dedicated security API
3. **Encryption Negotiation**: Agents agree on encryption method, key length, and parameters
4. **Challenge-Response Verification**: Mutual authentication through challenge-response mechanism
5. **Session Establishment**: Creation of session keys for ongoing encrypted communication

### Compatibility

The proposed protocol is designed with compatibility in mind:

- **Backward Compatibility**: Agents can maintain legacy protocol support while adopting A2A security
- **Progressive Implementation**: Can be deployed alongside existing systems and gradually adopted
- **SDK Integration**: Protocol can be implemented in agent SDKs for simplified adoption
- **Bridge Components**: Adapters can be developed for agents that cannot directly implement the protocol

## Questions and Discussion Topics

1. New eadge of AGIs need new version of prtocols, what do you think about it?
