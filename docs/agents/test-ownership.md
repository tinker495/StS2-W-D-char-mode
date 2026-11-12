# Test Ownership and Change Authority

These rules govern every repository test and acceptance asset. [AGENTS.md](/AGENTS.md#test-ownership-and-change-authority) carries the always-loaded summary; this document is the single source of truth.

## Principle

Tests are executable product specifications owned by the user, not implementation accessories. The agent implements the product; it does not decide what qualifies as correct.

Agents may read and run tests. They may add tests only when a human explicitly instructs them to add tests, and only within the scope of that instruction. Agents may recommend additional tests, but may not add them on their own judgment. Requests to implement features, fix bugs, refactor, or make tests pass do not by themselves authorize test additions or changes to acceptance criteria. Existing tests may not be modified, deleted, moved, or automatically regenerated.

Every test must identify its human author or, for an agent-written test, the human who explicitly requested its addition. Include comments explaining the request and the context for adding the test in detail.

## Protected Assets

Treat all repository tests and assets that determine product acceptance as protected, regardless of their path or name. This includes:

- Test code, assertions, test cases, and parameters.
- Expected values, fixtures, snapshots, golden data, and reference results.
- Independent validators and oracles, evaluation data, acceptance thresholds, and tolerances.
- Test collection, execution, and exclusion settings, and CI validation stages.

Agents may not exempt assets by relabeling them as internal unit tests, regression tests, or test improvements.

## Allowed Work

- Read and run existing tests and validation assets to understand requirements and failures.
- Add tests explicitly requested by a human, within the authorized scope defined above.
- Modify product implementation to satisfy user requirements.
- Create temporary reproductions, exploratory scripts, and observation logs for debugging.
- Propose necessary test cases, counterexamples, and specification ambiguities to the user in text.

Place temporary verification code only in temporary locations outside formal test directories. Do not commit it, register it with CI, or promote it to formal tests. Its success does not constitute user approval or product acceptance.

## Prohibited Work

- Adding tests without an explicit human instruction, or beyond the scope of that instruction.
- Modifying, deleting, moving, or automatically regenerating existing tests.
- Treating current implementation output as the correct answer and fixing expected values or snapshots to match it.
- Weakening assertions, widening tolerances, removing cases, adding skip/xfail handling, changing fixtures, or bypassing validation stages to make tests pass.
- Detecting the test environment or specific test inputs to hardcode answers, or hiding failures, violations, or omissions to pass.
- Relaxing this policy or related protections for convenience.

## Failures and Specification Conflicts

Investigate implementation defects first when a test fails. Failure alone does not establish that a test is wrong. Conversely, an existing test is not automatically a user-approved product specification.

When a test conflicts with user requirements or there is insufficient evidence to establish expected behavior, do not change the test or preserve incorrect implementation behavior. Report the conflicting requirements, reproduction input, actual result, and required user decision. Hold only the changes that depend on that decision.

When additional tests appear necessary, propose the input conditions, behavior to verify, and evidence supporting the expected result to the user. Wait for an explicit human instruction to add those tests before implementing them.

## Completion Report

Distinguish the scope and results of tests actually run, verification not performed, remaining failures, and decisions needed from the user.
