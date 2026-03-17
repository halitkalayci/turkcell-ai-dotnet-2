---
name: fix-error
description: Herhangi bir compile-time hata alındığında bu prompta istenilen bilgiler verilerek, hatanın çözülmesi beklenir.
argument-hint: Hatayı yapıştırın..
---

### Kullanıcıdan beklenecek girdiler:

- Build Output

- İlgili kod bloğu (opsiyonel)

### Prompt:

We have an error.

Constraints:
- Do NOT add new dependencies
- Do NOT refactor unrelated code
- Minimal diff only

Task:

1) Identify root cause from the FIRST error.
2) Propose the smallest fix
3) Provide the corrected code for the affected file only

Here is the build output:
<input-1>

Here is the relevant code snippet:
<input-2>