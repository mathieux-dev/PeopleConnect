export const formatCPF = (value: string): string => {
  const cleanValue = value.replace(/\D/g, '');
  
  if (cleanValue.length <= 11) {
    return cleanValue
      .replace(/(\d{3})(\d)/, '$1.$2')
      .replace(/(\d{3})(\d)/, '$1.$2')
      .replace(/(\d{3})(\d{1,2})$/, '$1-$2');
  }
  
  return value;
};

export const formatPhone = (value: string): string => {
  const cleanValue = value.replace(/\D/g, '');
  
  if (cleanValue.length <= 11) {
    if (cleanValue.length <= 10) {
      return cleanValue
        .replace(/(\d{2})(\d)/, '($1) $2')
        .replace(/(\d{4})(\d)/, '$1-$2');
    } else {
      return cleanValue
        .replace(/(\d{2})(\d)/, '($1) $2')
        .replace(/(\d{5})(\d)/, '$1-$2');
    }
  }
  
  return value;
};

export const formatDate = (value: string): string => {
  const cleanValue = value.replace(/\D/g, '');
  
  if (cleanValue.length <= 8) {
    return cleanValue
      .replace(/(\d{2})(\d)/, '$1/$2')
      .replace(/(\d{2})(\d)/, '$1/$2');
  }
  
  return value;
};

export const formatDateForAPI = (dateString: string): string => {
  // Convert DD/MM/YYYY to YYYY-MM-DD
  const [day, month, year] = dateString.split('/');
  return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
};

export const formatDateFromAPI = (dateString: string): string => {
  // Convert YYYY-MM-DD to DD/MM/YYYY
  const [year, month, day] = dateString.split('T')[0].split('-');
  return `${day}/${month}/${year}`;
};

export const censorContact = (contact: string, type: string): string => {
  if (type.toLowerCase().includes('email')) {
    const [local, domain] = contact.split('@');
    if (local && domain) {
      const censoredLocal = local.charAt(0) + '*'.repeat(Math.max(0, local.length - 2)) + (local.length > 1 ? local.charAt(local.length - 1) : '');
      const [domainName, extension] = domain.split('.');
      const censoredDomain = '*'.repeat(domainName.length);
      return `${censoredLocal}@${censoredDomain}.${extension}`;
    }
  } else if (type.toLowerCase().includes('telefone') || type.toLowerCase().includes('whatsapp')) {
    // Format: (11) 9****-****
    const cleanPhone = contact.replace(/\D/g, '');
    if (cleanPhone.length >= 10) {
      const ddd = cleanPhone.substring(0, 2);
      const firstDigit = cleanPhone.substring(2, 3);
      const lastDigits = cleanPhone.substring(cleanPhone.length - 4);
      return `(${ddd}) ${firstDigit}****-****`;
    }
  }
  
  // Generic censoring for other types
  if (contact.length <= 3) {
    return '*'.repeat(contact.length);
  }
  
  return contact.charAt(0) + '*'.repeat(contact.length - 2) + contact.charAt(contact.length - 1);
};

export const getContactIcon = (type: string): string => {
  const lowerType = type.toLowerCase();
  
  if (lowerType.includes('email')) return '📧';
  if (lowerType.includes('telefone')) return '📱';
  if (lowerType.includes('whatsapp')) return '💬';
  if (lowerType.includes('linkedin')) return '💼';
  if (lowerType.includes('instagram')) return '📷';
  if (lowerType.includes('facebook')) return '👥';
  
  return '📞'; // Default contact icon
};
