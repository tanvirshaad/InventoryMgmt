# -*- coding: utf-8 -*-

from odoo import api, fields, models, _
from odoo.exceptions import UserError, ValidationError
import requests
import logging

_logger = logging.getLogger(__name__)

class ImportInventoryWizard(models.TransientModel):
    _name = 'inventory.connector.import.wizard'
    _description = 'Import Inventory Wizard'

    api_token = fields.Char('API Token', required=True)
    api_url = fields.Char('API URL', required=True, default='https://yourdomain.com/api/InventoryApi')
    
    def action_import_inventory(self):
        """Import inventory data from external API"""
        self.ensure_one()
        
        if not self.api_token:
            raise ValidationError(_("API Token cannot be empty"))
            
        # Check if inventory with this token already exists
        existing = self.env['inventory.connector.inventory'].search([('api_token', '=', self.api_token)], limit=1)
        if existing:
            raise ValidationError(_("An inventory with this API token already exists"))
            
        try:
            # First, check if the token is valid by getting basic info
            info_url = f"{self.api_url}/info"
            response = requests.get(info_url, params={'token': self.api_token}, timeout=10)
            
            if response.status_code != 200:
                raise UserError(_("Invalid API token or URL. Server returned: %s") % response.text)
                
            info_data = response.json()
            
            # Create the inventory
            inventory = self.env['inventory.connector.inventory'].create({
                'name': info_data.get('title', 'Imported Inventory'),
                'description': info_data.get('description'),
                'external_id': info_data.get('id'),
                'api_token': self.api_token,
                'api_url': self.api_url,
                'category': info_data.get('category'),
                'is_public': info_data.get('isPublic', False),
                'created_at': info_data.get('createdAt'),
                'updated_at': info_data.get('updatedAt'),
            })
            
            # Immediately sync to get all data
            inventory.action_sync_inventory()
            
            # Show the new inventory record
            return {
                'name': _('Imported Inventory'),
                'view_mode': 'form',
                'res_model': 'inventory.connector.inventory',
                'res_id': inventory.id,
                'type': 'ir.actions.act_window',
            }
            
        except Exception as e:
            _logger.error("Error importing inventory: %s", str(e))
            raise UserError(_("Error importing inventory: %s") % str(e))