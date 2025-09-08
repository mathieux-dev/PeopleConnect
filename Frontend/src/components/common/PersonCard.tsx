import React from 'react';
import { Eye, User } from 'lucide-react';
import { PersonResponseDto } from '../../types/api.types';
import { formatDateFromAPI, censorContact, getContactIcon } from '../../utils/formatters';
import { useAuth } from '../../hooks/useAuth';

interface PersonCardProps {
  person: PersonResponseDto;
  onViewDetails: (person: PersonResponseDto) => void;
}

export const PersonCard: React.FC<PersonCardProps> = ({ person, onViewDetails }) => {
  const { isAuthenticated } = useAuth();

  return (
    <div className="bg-white rounded-xl shadow-md hover:shadow-lg transition-all duration-300 p-6 border border-gray-200 hover:border-blue-300">
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
            <User className="w-6 h-6 text-blue-600" />
          </div>
          <div>
            <h3 className="font-semibold text-gray-900 text-lg">{person.nome}</h3>
            <p className="text-gray-600 text-sm">CPF: {person.cpf}</p>
          </div>
        </div>
        <span className={`px-2 py-1 rounded-full text-xs font-medium ${
          person.sexo === 'M' 
            ? 'bg-blue-100 text-blue-800' 
            : person.sexo === 'F'
            ? 'bg-pink-100 text-pink-800'
            : 'bg-gray-100 text-gray-800'
        }`}>
          {person.sexo === 'M' ? 'Masculino' : person.sexo === 'F' ? 'Feminino' : 'Não Informado'}
        </span>
      </div>

      {/* Personal Info */}
      <div className="space-y-2 mb-4">
        <p className="text-sm text-gray-600">
          <span className="font-medium">Nascimento:</span> {formatDateFromAPI(person.dataNascimento)}
        </p>
        {person.naturalidade && (
          <p className="text-sm text-gray-600">
            <span className="font-medium">Naturalidade:</span> {person.naturalidade}
          </p>
        )}
        {person.nacionalidade && (
          <p className="text-sm text-gray-600">
            <span className="font-medium">Nacionalidade:</span> {person.nacionalidade}
          </p>
        )}
      </div>

      {/* Contacts */}
      {person.contacts && person.contacts.length > 0 && (
        <div className="space-y-2 mb-4">
          <h4 className="font-medium text-gray-900 text-sm">Contatos:</h4>
          <div className="space-y-1">
            {person.contacts.slice(0, 2).map((contact) => (
              <div key={contact.id} className="flex items-center gap-2 text-sm text-gray-600">
                <span>{getContactIcon(contact.type)}</span>
                <span className="font-medium">{contact.type}:</span>
                <span className={isAuthenticated ? '' : 'font-mono'}>
                  {isAuthenticated ? contact.value : censorContact(contact.value, contact.type)}
                </span>
              </div>
            ))}
            {person.contacts.length > 2 && (
              <p className="text-xs text-gray-500">
                +{person.contacts.length - 2} contato(s) adicional(is)
              </p>
            )}
          </div>
        </div>
      )}

      {/* Action Button */}
      <button
        onClick={() => onViewDetails(person)}
        disabled={!isAuthenticated}
        className={`w-full flex items-center justify-center gap-2 py-2 px-4 rounded-lg font-medium transition-colors duration-200 ${
          isAuthenticated
            ? 'bg-blue-600 hover:bg-blue-700 text-white'
            : 'bg-gray-100 text-gray-400 cursor-not-allowed'
        }`}
      >
        <Eye className="w-4 h-4" />
        {isAuthenticated ? 'Ver Detalhes' : 'Login necessário'}
      </button>
    </div>
  );
};
