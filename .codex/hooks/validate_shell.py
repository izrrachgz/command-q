#!/usr/bin/env python3
"""Check the PreToolUse shell payload and emit a denial for known patterns.

The hook receives one JSON event on stdin. It never logs the command or its
arguments. It screens common command forms as text. The sandbox and execution
policy remain responsible for enforcing access.
"""

from __future__ import annotations

import json
import re
import sys


COMMAND_START = r"(?:^|[;&|\r\n])\s*"
COMMAND_END = r"(?=\s|[;&|]|$)"
BLOCKED = (
    (
        r"""rm\s+-(?:rf|fr)\s+(?:--\s+)?(?:/|~|\$\{?HOME\}?|"\$\{?HOME\}?")""",
        "Recursive deletion of a filesystem or home root is blocked.",
    ),
    (
        r"git(?:\.exe)?\s+reset\s+--hard",
        "git reset --hard can discard uncommitted work.",
    ),
    (
        r"git(?:\.exe)?\s+clean\s+(?:-[a-z]+\s+)*-[a-z]*f[a-z]*",
        "Forced git clean can delete untracked work.",
    ),
    (
        r"Remove-Item(?=[^\r\n;&|]*\s-Recurse\b)(?=[^\r\n;&|]*\s-Force\b)",
        "Recursive forced deletion is blocked. Use exact, verified targets.",
    ),
    (
        r"(?:format(?:\.com|\.exe)?|Format-Volume)",
        "Filesystem format commands are blocked.",
    ),
)


def main() -> int:
    try:
        payload = json.load(sys.stdin)
    except (json.JSONDecodeError, UnicodeDecodeError):
        return 0

    tool_input = payload.get("tool_input") if isinstance(payload, dict) else None
    command = tool_input.get("command") if isinstance(tool_input, dict) else None
    if not isinstance(command, str):
        return 0

    for pattern, reason in BLOCKED:
        if re.search(COMMAND_START + pattern + COMMAND_END, command, re.IGNORECASE):
            json.dump(
                {
                    "hookSpecificOutput": {
                        "hookEventName": "PreToolUse",
                        "permissionDecision": "deny",
                        "permissionDecisionReason": reason,
                    }
                },
                sys.stdout,
            )
            break

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
