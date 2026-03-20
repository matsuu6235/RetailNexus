export function normalizePhoneNumber(value: string): string {
    return value.replace(/-/g, "").trim();
  }
  
  export function formatPhoneNumber(value?: string | null): string {
    if (!value) return "";
  
    const digits = value.replace(/\D/g, "");
  
    if (digits.length === 10) {
      return `${digits.slice(0, 2)}-${digits.slice(2, 6)}-${digits.slice(6)}`;
    }
  
    if (digits.length === 11) {
      return `${digits.slice(0, 3)}-${digits.slice(3, 7)}-${digits.slice(7)}`;
    }
  
    return value;
  }